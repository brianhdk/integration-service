using System;
using System.IO;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Infrastructure.IO
{
	internal class WaitForEscapeKey : IWaitForShutdownRequest
	{
		private readonly IConsoleWriter _console;

		public WaitForEscapeKey(IConsoleWriter console)
		{
			_console = console;
		}

		public void Wait()
		{
			_console.WaitUntilEscapeKeyIsHit();
		}
	}
}