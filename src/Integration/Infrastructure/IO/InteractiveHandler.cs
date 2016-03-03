using System.IO;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Infrastructure.IO
{
	public class InteractiveHandler : IProcessExitHandler
	{
		private readonly TextWriter _outputter;

		public InteractiveHandler(TextWriter outputter)
		{
			_outputter = outputter;
		}

		public void Wait()
		{
			_outputter.WaitUntilEscapeKeyIsHit();
		}
	}
}