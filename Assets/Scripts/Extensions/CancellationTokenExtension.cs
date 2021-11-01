using System.Threading;

namespace VoxCake.Extensions
{
	public static class CancellationTokenExtension
	{
		public static CancellationToken LinkWith(
			this CancellationToken cancellationToken,
			CancellationToken otherCancellationToken)
		{
			var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
				cancellationToken,
				otherCancellationToken);

			return linkedTokenSource.Token;
		}
	}
}