using System;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;
using VoxCake.Extensions;
using VoxCake.Packet;

namespace VoxCake.Network
{
    public class NetworkClient : IDisposable
    {
        private readonly uint _appId;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _clientCancellationToken;

        private bool _isInitialized;

        public NetworkClient(uint appId)
        {
            _appId = appId;
            _cancellationTokenSource = new CancellationTokenSource();
            _clientCancellationToken = _cancellationTokenSource.Token;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            cancellationToken = cancellationToken.LinkWith(_clientCancellationToken);

            while (!TryConnectToSteam(_appId) && !cancellationToken.IsCancellationRequested)
            {
                await Task.Yield();
            }

            cancellationToken.ThrowIfCancellationRequested();

            SteamNetworkingUtils.InitRelayNetworkAccess();
            PlayerLoopAsync(cancellationToken).RunAsynchronously(Debug.LogException);

            _isInitialized = true;
        }

        public async Task<NetworkMatch> CreateMatchAsync(NetworkMatchOptions matchOptions, CancellationToken cancellationToken)
        {
            ThrowIfNotInitialized();

            cancellationToken = cancellationToken.LinkWith(_clientCancellationToken);

            var lobbyResult = await SteamMatchmaking.CreateLobbyAsync(matchOptions.maxPlayers);

            cancellationToken.ThrowIfCancellationRequested();

            if (lobbyResult.HasValue)
            {
                var lobby = lobbyResult.Value;

                var match = new NetworkMatch(lobby);

                return match;
            }

            throw new NetworkMatchCreateException();
        }

        public async Task<NetworkMatch> JoinMatchAsync(NetworkMatchInfo networkMatchInfo, IPacketProtocol protocol,
            CancellationToken cancellationToken)
        {
            ThrowIfNotInitialized();

            cancellationToken = cancellationToken.LinkWith(_clientCancellationToken);

            var targetLobby = networkMatchInfo.Lobby;
            var joinResult = await targetLobby.Join();

            cancellationToken.ThrowIfCancellationRequested();

            var joinResultCode = (int)joinResult;

            if (joinResultCode == 1)
            {
                var match = new NetworkMatch(targetLobby);

                return match;
            }

            var joinExceptionReason = (NetworkMatchJoinException.Reason) joinResultCode;
            throw new NetworkMatchJoinException(joinExceptionReason);
        }

        public Task<NetworkMatchInfo[]> GetMatchListAsync(CancellationToken cancellationToken)
        {
            ThrowIfNotInitialized();

            cancellationToken = cancellationToken.LinkWith(_clientCancellationToken);

            var taskCompletionSource = new TaskCompletionSource<NetworkMatchInfo[]>();
            var task = (Task) null;

            using (cancellationToken.Register(OnFailed))
            {
                task = SteamMatchmaking.LobbyList.RequestAsync()
                    .ContinueWith(resultTask =>
                    {
                        var lobbies = resultTask.Result;

                        if (lobbies != null)
                        {
                            var lobbiesCount = lobbies.Length;
                            var networkMatchInfos = new NetworkMatchInfo[lobbiesCount];

                            for (var i = 0; i < lobbiesCount; i++)
                            {
                                networkMatchInfos[i] = new NetworkMatchInfo(lobbies[i]);
                            }

                            taskCompletionSource.TrySetResult(networkMatchInfos);
                        }
                    }, cancellationToken);

                task.RunAsynchronously(exception => { taskCompletionSource.TrySetException(exception); });
            }
            
            void OnFailed()
            {
                task?.Dispose();
                taskCompletionSource.TrySetCanceled();
            }

            return taskCompletionSource.Task;
        }

        private async Task PlayerLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
#if !UNITY_EDITOR
					if (SteamClient.RestartAppIfNecessary(_appId))
					{
						Application.Quit();
					}
#endif
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }

                await Task.Yield();
            }
        }

        private bool TryConnectToSteam(uint appId)
        {
            try
            {
                SteamClient.Init(appId);
            }
            catch (Exception exception)
            {
                if (exception is DllNotFoundException)
                {
                    throw;
                }
            }

            return SteamClient.IsValid && SteamClient.IsLoggedOn;
        }

        private void ThrowIfNotInitialized()
        {
            if (!_isInitialized)
            {
                throw new NetworkClientNotInitializedException();
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            SteamClient.Shutdown();
        }
    }
}