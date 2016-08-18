using System;
using System.IO;
using System.Threading;
using Vertica.Integration.Domain.LiteServer;

namespace Experiments.LiteServer
{
	public class CustomWorker : IBackgroundWorker
	{
		private readonly TextWriter _writer;

		public CustomWorker(TextWriter writer)
		{
			_writer = writer;
		}

		public TimeSpan Work(CancellationToken token, BackgroundWorkerContext context)
		{
			_writer.WriteLine("Ping!");

			if (context.InvocationCount == 5)
				return context.Exit();

			return TimeSpan.FromSeconds(1);
		}
	}
}