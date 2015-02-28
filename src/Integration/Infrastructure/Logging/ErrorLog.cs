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

        public virtual int Id { get; protected set; }
		public virtual string MachineName { get; protected set; }
		public virtual string IdentityName { get; protected set; }
		public virtual string CommandLine { get; protected set; }
        public virtual Severity Severity { get; protected set; }
		public virtual string Message { get; protected set; }
		public virtual string FormattedMessage { get; protected set; }
		public virtual DateTimeOffset TimeStamp { get; protected set; }
        public virtual Target Target { get; protected set; }

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