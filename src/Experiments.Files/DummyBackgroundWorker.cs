using System;
using System.IO;
using System.Threading;
using Vertica.Integration.Domain.LiteServer;

namespace Experiments.Files
{
	internal class DummyBackgroundWorker : IBackgroundWorker
	{
		private readonly TextWriter _outputter;

		public DummyBackgroundWorker(TextWriter outputter)
		{
			_outputter = outputter;
		}

		public TimeSpan Work(BackgroundWorkContext context)
		{
			_outputter.WriteLine(Thread.CurrentThread.ManagedThreadId);

			if (context.InvocationCount == 5)
				return context.Exit();

			return TimeSpan.FromSeconds(1);
		}
	}
}