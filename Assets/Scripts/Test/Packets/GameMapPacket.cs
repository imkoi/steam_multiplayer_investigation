using Test.Data;
using VoxCake.Packet;

namespace Test
{
	public class GameMapPacket : Packet
	{
		public Vector3IntData position;
		public byte colorIndex;
	}
}