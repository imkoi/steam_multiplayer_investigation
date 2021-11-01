using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace VoxCake.Network
{
	public class NetworkTimer
	{
		public event Action Finish; 
		
		private readonly Stopwatch _stopwatch;
		
		public NetworkTimer()
		{
			_stopwatch = new Stopwatch();
		}

		public async Task WaitAsync(float seconds, CancellationToken cancellationToken)
		{
			_stopwatch.Restart();

			while (_stopwatch.ElapsedMilliseconds / 1000f < seconds && !cancellationToken.IsCancellationRequested)
			{
				await Task.Yield();
			}
			
			Finish?.Invoke();
		}
	}
}