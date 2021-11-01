using System.Threading;
using Test;
using UnityEngine;
using VoxCake.Network;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private GameMatchListView _matchList;
    
    private CancellationTokenSource _cancellationTokenSource;
    private NetworkClient _networkClient;
    
    private async void OnEnable()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _networkClient = new NetworkClient(480);
        
        var cancellationToken = _cancellationTokenSource.Token;

        await _networkClient.InitializeAsync(cancellationToken);
        Debug.Log("CONNECTED TO STEAM");
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
        _cancellationTokenSource?.Cancel();
        _networkClient?.Dispose();
    }
}
