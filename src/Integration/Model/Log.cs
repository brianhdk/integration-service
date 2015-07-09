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
            if (message == null) throw new ArgumentNullException("message");
            if (logger == null) throw new ArgumentNullException("logger");

            _message = message;
            _logger = logger;

            //Func<ITarget, string, ErrorLog> logWarning = (target, message) => ;
            //Func<ITarget, string, ErrorLog> logError = (target, message) => _logger.LogError(target, message);
            //Func<Exception, ITarget, ErrorLog> logException = (exception, target) => _logger.LogError(exception, target);
        }

        public void Message(string format, params object[] args)
		{
			_message(String.Format(format, args));
		}

	    public ErrorLog Warning(ITarget target, string format, params object[] args)
	    {
	        string message = String.Format(format, args);

	        _message(String.Format("[WARNING] {0}", message));
	        return _logger.LogWarning(target, message);
	    }

	    public ErrorLog Error(ITarget target, string format, params object[] args)
	    {
            string message = String.Format(format, args);

            _message(String.Format("[ERROR] {0}", message));
	        return _logger.LogError(target, message);
	    }

        public ErrorLog Exception(Exception exception, ITarget target = null)
        {
            if (exception == null) throw new ArgumentNullException("exception");

            _message(String.Format("[ERROR] {0}", exception.Message));
            return _logger.LogError(exception, target);
        }
	}
}