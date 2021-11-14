using System;
using System.Threading.Tasks;

namespace VoxCake.Extensions
{
	public static class TaskExtension
	{
		public static async void RunAsynchronously(this Task task, Action<Exception> failedCallback)
		{
			try
			{
				await task;
			}
			catch (Exception exception)
			{
				failedCallback?.Invoke(exception);
			}
		}
	}
}