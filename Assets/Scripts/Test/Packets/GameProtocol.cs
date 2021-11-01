using VoxCake.Packet;

namespace Test
{
	public class GameProtocol : PacketProtocol
	{
		public GameProtocol()
		{
			BindPacket<GameMapPacket>();
			BindPacket<GamePlayerPacket>();
		}
	}
}