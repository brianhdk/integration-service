using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Experiments.Files
{
	internal class DummyBackgroundOperation : IBackgroundOperation
	{
		private readonly TextWriter _outputter;

		public DummyBackgroundOperation(TextWriter outputter)
		{
			_outputter = outputter;
		}

		public Task Create(CancellationToken token)
		{
			return Task.Run(() =>
			{
				_outputter.WriteLine("Running.");
				token.WaitHandle.WaitOne();
				_outputter.WriteLine("Stopped!");

			}, token);
		}
	}
}