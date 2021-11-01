using Test.Data;
using VoxCake.Packet;

namespace Test
{
	public class GamePlayerPacket : Packet // size 2 + 6 + 6 bytes
	{
		public TimestampData timestamp;
		public Vector3FixedData position;
		public Vector3FixedData rotation;
	}
}