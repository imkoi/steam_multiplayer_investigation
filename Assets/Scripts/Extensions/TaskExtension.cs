using System.Threading.Tasks;

namespace VoxCake.Extensions
{
	public static class TaskExtension
	{
		public static async void RunAsynchronously(this Task task)
		{
			await task;
		}
	}
}