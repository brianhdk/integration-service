using System;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model
{
    internal class Log : ILog
	{
		private readonly Action<string> _message;
	    private readonly Action<Target, string> _warning;
	    private readonly Action<Target, string> _error;

	    internal Log(Action<string> message, Action<Target, string> warning, Action<Target, string> error)
		{
			if (message == null) throw new ArgumentNullException("message");
	        if (warning == null) throw new ArgumentNullException("warning");
	        if (error == null) throw new ArgumentNullException("error");

	        _message = message;
	        _warning = warning;
	        _error = error;
		}

		public void Message(string format, params object[] args)
		{
			_message(String.Format(format, args));
		}

	    public void Warning(Target target, string format, params object[] args)
	    {
	        string message = String.Format(format, args);

	        _message(String.Format("[WARNING] {0}", message));
	        _warning(target, message);
	    }

	    public void Error(Target target, string format, params object[] args)
	    {
            string message = String.Format(format, args);

            _message(String.Format("[ERROR] {0}", message));
	        _error(target, String.Format(format, args));
	    }
	}
}