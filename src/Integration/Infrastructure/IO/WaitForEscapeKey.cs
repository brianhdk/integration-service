using System.IO;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Infrastructure.IO
{
	internal class WaitForEscapeKey : IWaitForShutdownRequest
	{
		private readonly TextWriter _writer;

		public WaitForEscapeKey(TextWriter writer)
		{
			_writer = writer;
		}

		public void Wait()
		{
			_writer.WaitUntilEscapeKeyIsHit();
		}
	}
}