using System;
using Hangfire.Server;

namespace Integration.Experiments.Hangfire
{
	public class CleanTempDirectoryProcess : IBackgroundProcess
	{
		public void Execute(BackgroundProcessContext context)
		{
			//Directory.CleanUp(Directory.GetTempDirectory());
			Console.WriteLine("Test {0}", DateTime.Now);
			context.Wait(TimeSpan.FromHours(1));
		}
	}
}