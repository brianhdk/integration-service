using System;

namespace Vertica.Integration.Model
{
	public class Log
	{
		private readonly Action<string> _logMessage;

		internal Log(Action<string> logMessage)
		{
			if (logMessage == null) throw new ArgumentNullException("logMessage");

			_logMessage = logMessage;
		}

		public void Message(string format, params object[] args)
		{
			_logMessage(String.Format(format, args));
		}
	}
}