using System;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Windows;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Logging
{
    public class ErrorLog
	{
		protected ErrorLog()
		{
		}

        public ErrorLog(Severity severity, string message, Target target)
        {
            Initialize(severity, message, message, target);
        }

        public ErrorLog(Exception exception, Target target = null)
		{
			if (exception == null) throw new ArgumentNullException("exception");

			Initialize(exception, target ?? Target.Service);
		}

        public int Id { get; internal set; }
		public string MachineName { get; private set; }
		public string IdentityName { get; private set; }
		public string CommandLine { get; private set; }
        public Severity Severity { get; private set; }
		public string Message { get; private set; }
		public string FormattedMessage { get; private set; }
		public DateTimeOffset TimeStamp { get; private set; }
        public Target Target { get; private set; }

        private void Initialize(Exception exception, Target target)
        {
            Initialize(Severity.Error, exception.Message, exception.GetFullStacktrace(), target);
        }

        private void Initialize(Severity severity, string message, string formattedMessage, Target target)
        {
            MachineName = Environment.MachineName;
            IdentityName = WindowsUtils.GetIdentityName();
            CommandLine = Environment.CommandLine.MaxLength(4000);
            Severity = severity;
            Message = message.MaxLength(4000);
            FormattedMessage = formattedMessage;
            TimeStamp = Time.UtcNow;
            Target = target;
        }
	}
}