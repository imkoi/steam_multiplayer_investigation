using System;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;

namespace VoxCake.Network
{
    public class NetworkMatchSocket : IDisposable
    {
        public event Action HostChanged;
        
        private Lobby _lobby;

        private NetworkConnectionManager _currentConnection;
        private NetworkSocket _currentSocket;
        private bool _isConnected;

        public NetworkMatchSocket(Lobby lobby)
        {
            _lobby = lobby;
            
            SteamNetworkingSockets.OnConnectionStatusChanged += OnConnectionStatusChanged;
        }

        public async Task ConnectionAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_isConnected)
                {
                    if (_lobby.Owner.IsMe && _currentSocket == null)
                    {
                        _currentSocket = SteamNetworkingSockets.CreateRelaySocket<NetworkSocket>();
                    }
                    else if (_currentConnection == null)
                    {
                        _currentConnection = SteamNetworkingSockets.ConnectRelay<NetworkConnectionManager>(_lobby.Owner.Id);
                    }
                }

                await Task.Yield();
            }
        }
        
        private void OnConnectionStatusChanged(Connection connection, ConnectionInfo connectionInfo)
        {
            if (connectionInfo.Identity.SteamId == _lobby.Owner.Id)
            {
                //SteamNetworkingSockets.OnConnectionStatusChanged -= OnConnectionStatusChanged;

                connection.Accept();
            }
        }

        public void Dispose()
        {
            SteamNetworkingSockets.OnConnectionStatusChanged -= OnConnectionStatusChanged;
            
            _currentConnection?.Close();
            _currentSocket?.Close();
        }
    }
}