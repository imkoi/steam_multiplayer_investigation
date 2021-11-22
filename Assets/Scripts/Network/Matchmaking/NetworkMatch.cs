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

		private readonly NetworkMatchSocket _matchSocket;
		private readonly CancellationTokenSource _matchCancellationTokenSource;

		private Lobby _currentLobby;
		private AccessibilityType _currentAccessibility;

		public NetworkMatch(Lobby currentLobby)
		{
			_currentLobby = currentLobby;
			_matchCancellationTokenSource = new CancellationTokenSource();
			
			SteamMatchmaking.OnChatMessage += OnChatMessageReceived;

			_matchSocket = new NetworkMatchSocket(currentLobby);
			_matchSocket.ConnectionAsync(_matchCancellationTokenSource.Token).RunAsynchronously(Debug.LogException);
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
			_matchSocket.Dispose();
		}
	}
}