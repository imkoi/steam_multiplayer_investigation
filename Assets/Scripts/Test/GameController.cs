using System.Threading;
using Test;
using UnityEngine;
using UnityEngine.UI;
using VoxCake.Network;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameMatchListView _matchList;
    [SerializeField] private Button _createMatchButton;
    
    private CancellationTokenSource _cancellationTokenSource;
    private NetworkClient _networkClient;
    private NetworkMatch _currentMatch;
    
    private async void OnEnable()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _networkClient = new NetworkClient(224540);
        
        var cancellationToken = _cancellationTokenSource.Token;

        _createMatchButton.interactable = false;
        
        await _networkClient.InitializeAsync(cancellationToken);
        Debug.Log("CONNECTED TO STEAM");
        
        _createMatchButton.onClick.AddListener(OnCreateMatchClicked);
        _createMatchButton.interactable = true;
        
        var matchList = await _networkClient.GetMatchListAsync(cancellationToken);
        
        if (matchList.Length > 0)
        {
            _matchList.Initialize(matchList, _networkClient);
        }
        else
        {
            Debug.Log("There is no match available");
        }
    }

    private void OnDisable()
    {
        _createMatchButton.onClick.RemoveListener(OnCreateMatchClicked);
        
        _cancellationTokenSource?.Cancel();
        _networkClient?.Dispose();
        _currentMatch?.Dispose();
    }

    private async void OnCreateMatchClicked()
    {
        _currentMatch = await _networkClient.CreateMatchAsync(_cancellationTokenSource.Token);
    }
}
