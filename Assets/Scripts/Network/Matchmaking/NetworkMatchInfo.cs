using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;

namespace VoxCake.Network
{
	public class NetworkMatchInfo
	{
		internal Lobby Lobby => _lobby;
		public int PlayersCount => _lobby.MemberCount;
		public int MaxPlayersCount => _lobby.MaxMembers;
		public IEnumerable<KeyValuePair<string, string>> Data => _lobby.Data;

		private readonly Lobby _lobby;

		public NetworkMatchInfo(Lobby lobby)
		{
			_lobby = lobby;
		}
	}
}