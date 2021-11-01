using System;

namespace VoxCake.Network
{
	public class NetworkMatchJoinException : Exception
	{
		public enum Reason
		{
			Unknown = 0,
			DoesntExist = 2,
			NotAllowed = 3,
			Full = 4,
			Error = 5,
			Banned = 6,
			Limited = 7,
			ClanDisabled = 8,
			CommunityBan = 9,
			MemberBlockedYou = 10,
			YouBlockedMember = 11,
			RatelimitExceeded = 15
		}

		public Reason ExceptionReason { get; }

		public NetworkMatchJoinException(Reason reason)
		{
			ExceptionReason = reason;
		}
	}
}