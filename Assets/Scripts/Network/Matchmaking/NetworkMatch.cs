using System;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using VoxCake.Extensions;

namespace VoxCake.Network
{
	public class NetworkMatch : IDisposable
	{
		public enum AccessibilityType
		{
			Invisible = 0,
			FriendsOnly = 1,
			Public = 2,
		}
		
		public event Action<NetworkMatchChatMessageData> ChatMessageReceived;
		
		internal Lobby CurrentLobby => _currentLobby;
		public Friend Owner => _currentLobby.Owner;

		public AccessibilityType Accessibility
		{
			get => _currentAccessibility;
			set
			{
				switch (value)
				{
					case AccessibilityType.Invisible:
						_currentLobby.SetInvisible();
						break;
					case AccessibilityType.FriendsOnly:
						_currentLobby.SetFriendsOnly();
						break;
					case AccessibilityType.Public:
						_currentLobby.SetPublic();
						break;
				}

				_currentAccessibility = value;
			}
		}
		
		private readonly CancellationTokenSource _matchCancellationTokenSource;

		private Lobby _currentLobby;
		private NetworkSocket _currentSocket;
		private AccessibilityType _currentAccessibility;
		private bool _isConnected;

		public NetworkMatch(Lobby currentLobby)
		{
			_currentLobby = currentLobby;
			_matchCancellationTokenSource = new CancellationTokenSource();
			
			SteamMatchmaking.OnChatMessage += OnChatMessageReceived;
			SteamNetworkingSockets.OnConnectionStatusChanged += OnConnectionStatusChanged;
			
			HandleConnection(_matchCancellationTokenSource.Token).RunAsynchronously(Debug.LogException);
		}
		
		private async Task HandleConnection(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				if (!_isConnected)
				{
					if (_currentLobby.Owner.IsMe)
					{
						_currentSocket = SteamNetworkingSockets.CreateRelaySocket<NetworkSocket>();
					}
					else
					{
						var connnection = SteamNetworkingSockets.ConnectRelay<NetworkConnectionManager>(_currentLobby.Owner.Id);
					}
				}
					
				await Task.Yield();
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
		
		private void OnConnectionStatusChanged(Connection connection, ConnectionInfo connectionInfo)
		{
			if (connectionInfo.Identity.SteamId == _currentLobby.Owner.Id)
			{
				//SteamNetworkingSockets.OnConnectionStatusChanged -= OnConnectionStatusChanged;

				connection.Accept();
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