using System;
using System.IO;
using Hangfire.Server;

namespace Integration.Experiments.Hangfire
{
	public class CleanTempDirectoryProcess : IBackgroundProcess
	{
		private readonly TextWriter _outputter;

		public CleanTempDirectoryProcess(TextWriter outputter)
		{
			_outputter = outputter;
		}

		public void Execute(BackgroundProcessContext context)
		{
			//Directory.CleanUp(Directory.GetTempDirectory());
			_outputter.WriteLine("Test {0}", DateTime.Now);
			context.Wait(TimeSpan.FromSeconds(10));
		}
	}
}