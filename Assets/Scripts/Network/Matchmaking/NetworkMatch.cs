using System;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using VoxCake.Extensions;

namespace VoxCake.Network
{
	public class NetworkMatch : IDisposable
	{
		public event Action<NetworkMatchChatMessageData> ChatMessageReceived;
		
		internal Lobby CurrentLobby => _currentLobby;
		public Friend Owner => _currentLobby.Owner;

		private Lobby _currentLobby;
		private NetworkSocket _currentSocket;

		private readonly CancellationTokenSource _matchCancellationTokenSource;
		
		public NetworkMatch(Lobby currentLobby)
		{
			_currentLobby = currentLobby;
			_matchCancellationTokenSource = new CancellationTokenSource();
			SteamMatchmaking.OnChatMessage += OnChatMessageReceived;
		}
		
		internal async Task ConnectAsync(CancellationToken cancellationToken)
		{
			var taskCompletionSource = new TaskCompletionSource<bool>();
			using var ctr = cancellationToken.Register(() =>
			{
				SteamNetworkingSockets.OnConnectionStatusChanged -= OnConnectionStatusChanged;
				taskCompletionSource.TrySetCanceled();
			});

			SteamNetworkingSockets.OnConnectionStatusChanged += OnConnectionStatusChanged;

			if (_currentLobby.Owner.IsMe)
			{
				_currentSocket = SteamNetworkingSockets.CreateRelaySocket<NetworkSocket>();
				HandleConnections(cancellationToken).RunSynchronously();
			}
			else
			{
				var connnection = SteamNetworkingSockets.ConnectRelay<NetworkConnectionManager>(_currentLobby.Owner.Id);
			}
			
			void OnConnectionStatusChanged(Connection connection, ConnectionInfo connectionInfo)
			{
				if (connectionInfo.Identity.SteamId == _currentLobby.Owner.Id)
				{
					//SteamNetworkingSockets.OnConnectionStatusChanged -= OnConnectionStatusChanged;

					connection.Accept();
				}
			}

			await taskCompletionSource.Task;
		}

		internal async Task HandleConnections(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				
			}
		}

		public void SendPacket()
		{
			SteamNetworking.SendP2PPacket(0, new byte[0], 0, 0, P2PSend.Unreliable);
		}

		private void SendChatMessage(string message)
		{
			_currentLobby.SendChatString(message);
		}

		private void OnChatMessageReceived(Lobby lobby, Friend sender, string message)
		{
			if (lobby.Id == _currentLobby.Id)
			{
				var chatDatagram = new NetworkMatchChatMessageData
				{
					sender = sender,
					message = message
				};
				
				ChatMessageReceived?.Invoke(chatDatagram);
			}
		}

		public void Dispose()
		{
			SteamMatchmaking.OnChatMessage -= OnChatMessageReceived;
			_matchCancellationTokenSource.Cancel();
			_currentSocket.Close();
		}
	}
}