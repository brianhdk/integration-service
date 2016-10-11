using System;
using System.IO;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Infrastructure.IO
{
	internal class WaitForEscapeKey : IWaitForShutdownRequest
	{
		private readonly TextWriter _outputter;

		public WaitForEscapeKey(TextWriter outputter)
		{
			_outputter = outputter;
		}

		public void Wait()
		{
			_outputter.WaitUntilEscapeKeyIsHit();
		}
	}
}