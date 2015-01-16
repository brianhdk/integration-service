using System;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model
{
	public class Log
	{
		private readonly Action<string> _message;
	    private readonly Action<Target, string> _warning;

	    internal Log(Action<string> message, Action<Target, string> warning)
		{
			if (message == null) throw new ArgumentNullException("message");
	        if (warning == null) throw new ArgumentNullException("warning");

	        _message = message;
	        _warning = warning;
		}

		public void Message(string format, params object[] args)
		{
			_message(String.Format(format, args));
		}

	    public void Warning(Target target, string format, params object[] args)
	    {
	        _warning(target, String.Format(format, args));
	    }
	}
}