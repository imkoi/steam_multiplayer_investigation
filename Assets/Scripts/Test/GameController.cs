using System;
using System.Threading;
using System.Threading.Tasks;
using Test;
using UnityEngine;
using UnityEngine.UI;
using VoxCake.Extensions;
using VoxCake.Network;

public class GameController : MonoBehaviour
{
    public enum SteamApplicationId
    {
        Spacewar = 480,
        AceOfSpades = 224540,
        SectorsEdge = 1024890,
        CrabGame = 1782210
    }
    
    [SerializeField] private SteamApplicationId _appId;
    [SerializeField] private GameMatchListView _matchList;
    [SerializeField] private Button _createMatchButton;
    [SerializeField] private Button _refreshButton;
    
    private CancellationTokenSource _cancellationTokenSource;
    private NetworkClient _networkClient;
    private NetworkMatch _currentMatch;

    private Task<NetworkMatchInfo[]> _currentMatchListRefreshTask;
    
    private async void OnEnable()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _networkClient = new NetworkClient((uint)_appId);
        
        var initializationCancellationTokenSource = new CancellationTokenSource();
        var initializationCancellationToken = _cancellationTokenSource.Token
            .LinkWith(initializationCancellationTokenSource.Token);
        
        initializationCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(5));
        
        SetButtonsActive(false);
        
        await _networkClient.InitializeAsync(initializationCancellationToken);

        SetButtonsActive(true);
        SubscribeOnEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();

        _cancellationTokenSource?.Cancel();
        _currentMatch?.Dispose();
        _networkClient?.Dispose();
    }
    
    private async Task<NetworkMatchInfo[]> GetMatchList(CancellationToken cancellationToken)
    {
        SetButtonsActive(false);

        var timeoutCancellationTokenSource = new CancellationTokenSource();
        timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(5));

        var linkedToken = cancellationToken.LinkWith(timeoutCancellationTokenSource.Token);

        try
        {
            return await _networkClient.GetMatchListAsync(linkedToken);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            return Array.Empty<NetworkMatchInfo>();
        }
        finally
        {
            SetButtonsActive(true);
        }
    }

    private void SubscribeOnEvents()
    {
        _createMatchButton.onClick.AddListener(OnCreateMatchClicked);
        _refreshButton.onClick.AddListener(OnRefreshButtonClicked);

        _matchList.JoinButtonClicked += OnJoinButtonClicked;
        _matchList.DataButtonClicked += OnMatchDataButtonClicked;
    }

    private void UnsubscribeFromEvents()
    {
        _createMatchButton.onClick.RemoveListener(OnCreateMatchClicked);
        _refreshButton.onClick.RemoveListener(OnRefreshButtonClicked);
        
        _matchList.JoinButtonClicked -= OnJoinButtonClicked;
        _matchList.DataButtonClicked -= OnMatchDataButtonClicked;
    }
    
    private void SetButtonsActive(bool isActive)
    {
        _createMatchButton.interactable = isActive;
        _refreshButton.interactable = isActive;
    }

    private async void OnCreateMatchClicked()
    {
        SetButtonsActive(false);

        try
        {
            _currentMatch = await _networkClient.CreateMatchAsync(new NetworkMatchOptions
            {
                maxPlayers = 250
            }, _cancellationTokenSource.Token);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            SetButtonsActive(true);
        }
    }

    private async void OnRefreshButtonClicked()
    {
        try
        {
            var matchList = await GetMatchList(_cancellationTokenSource.Token);

            if (matchList.Length > 0)
            {
                _matchList.Refresh(matchList);
            }
            else
            {
                Debug.Log("There is no match available");
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }
    
    private async void OnJoinButtonClicked(NetworkMatchInfo matchInfo)
    {
        var matchProtocol = new GameProtocol();
        
        var match = await _networkClient.JoinMatchAsync(matchInfo, matchProtocol, _cancellationTokenSource.Token);
    }
    
    private void OnMatchDataButtonClicked(NetworkMatchInfo matchInfo)
    {
        foreach (var pair in matchInfo.Data)
        {
            Debug.Log($"{pair.Key}:{pair.Value}");
        }
    }
}
