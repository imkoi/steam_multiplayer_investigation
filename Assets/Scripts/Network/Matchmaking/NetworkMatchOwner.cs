using Steamworks.Data;

namespace VoxCake.Network
{
	public class NetworkMatchOwner
	{
		public ulong Id => _lobby.Id.Value;
		public string Name => _lobby.Owner.Name;
		
		private readonly Lobby _lobby;

		public NetworkMatchOwner(Lobby lobby)
		{
			_lobby = lobby;
		}
	}
}