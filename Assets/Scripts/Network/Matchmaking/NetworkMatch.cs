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
		
		public NetworkMatch(Lobby currentLobby)
		{
			_currentLobby = currentLobby;
			SteamMatchmaking.OnChatMessage += OnChatMessageReceived;
		}

		public void SendPacket()
		{
			SteamNetworking.SendP2PPacket(0, new byte[0], 0, 0, P2PSend.Unreliable);
		}

		private void SendChatMessage(string message)
		{
			_currentLobby.SendChatString(message);
		}

		internal async Task ConnectAsync(CancellationToken cancellationToken)
		{
			var connectTimer = new NetworkTimer();
			var timerCancellationSource = new CancellationTokenSource();
			var timerCancellationToken = cancellationToken.LinkWith(timerCancellationSource.Token);
			connectTimer.Finish += OnTimeout;
			SteamNetworkingSockets.OnConnectionStatusChanged += OnConnectionStatusChanged;
			
			//connectTimer.WaitAsync(0.5f, timerCancellationToken).RunAsynchronously();
			
			if (_currentLobby.Owner.IsMe)
			{
				var socket = SteamNetworkingSockets.CreateRelaySocket<NetworkSocket>();
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
			
			void OnTimeout()
			{
				connectTimer.Finish -= OnTimeout;
				timerCancellationSource.Cancel();
			}
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
		}
	}
}