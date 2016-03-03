using System.IO;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Infrastructure.IO
{
	internal class DefaultHandler : IProcessExitHandler
	{
		private readonly TextWriter _outputter;

		public DefaultHandler(TextWriter outputter)
		{
			_outputter = outputter;
		}

		public void Wait()
		{
			_outputter.WaitUntilEscapeKeyIsHit();
		}
	}
}