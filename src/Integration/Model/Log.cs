using System;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model
{
    internal class Log : ILog
	{
        private readonly ILogger _logger;
        private readonly Action<string> _message;

        internal Log(Action<string> message, ILogger logger)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _message = message;
            _logger = logger;
        }

        public void Message(string format, params object[] args)
		{
			if (args != null && args.Length > 0)
			{
				_message(string.Format(format, args));
			}
			else
			{
				_message(format);
			}
		}

	    public ErrorLog Warning(ITarget target, string format, params object[] args)
	    {
	        string message = format;

		    if (args != null && args.Length > 0)
				message = string.Format(message, args);

	        _message($"[WARNING] {message}");
	        return _logger.LogWarning(target, message);
	    }

	    public ErrorLog Error(ITarget target, string format, params object[] args)
	    {
			string message = format;

			if (args != null && args.Length > 0)
				message = string.Format(message, args);

            _message($"[ERROR] {message}");
	        return _logger.LogError(target, message);
	    }

        public ErrorLog Exception(Exception exception, ITarget target = null)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            _message($"[ERROR] {exception.Message}");
            return _logger.LogError(exception, target);
        }
	}
}