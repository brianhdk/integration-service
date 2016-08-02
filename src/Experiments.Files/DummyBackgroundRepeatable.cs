using System;
using System.IO;
using System.Threading;

namespace Experiments.Files
{
	public class DummyBackgroundRepeatable : IBackgroundRepeatable
	{
		private readonly TextWriter _outputter;

		public DummyBackgroundRepeatable(TextWriter outputter)
		{
			_outputter = outputter;
		}

		public TimeSpan Work(BackgroundRepeatedContext context)
		{
			_outputter.WriteLine(Thread.CurrentThread.ManagedThreadId);

			if (context.InvocationCount == 5)
				return context.Exit();

			return TimeSpan.FromSeconds(1);
		}
	}
}