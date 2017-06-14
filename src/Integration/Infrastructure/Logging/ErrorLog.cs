using System;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Windows;
using Vertica.Utilities;

namespace Vertica.Integration.Infrastructure.Logging
{
    public class ErrorLog
    {
        private ErrorLog(Severity severity, ITarget target)
        {
            Severity = severity;
            Target = (target ?? Target.Service).Name;

            MachineName = Environment.MachineName;
            IdentityName = WindowsUtils.GetIdentityName();
            CommandLine = Environment.CommandLine.MaxLength(4000);
            TimeStamp = Time.UtcNow;
        }

        public ErrorLog(Severity severity, string message, ITarget target)
            : this(severity, target)
        {
            Message = message.MaxLength(4000);
            FormattedMessage = message;
        }

        public ErrorLog(Exception exception, ITarget target = null)
            : this(Severity.Error, target)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            Message = exception.Message.MaxLength(4000);
            FormattedMessage = exception.GetFullStacktrace();
        }

        public string Id { get; internal set; }
        public string MachineName { get; private set; }
        public string IdentityName { get; private set; }
        public string CommandLine { get; private set; }
        public Severity Severity { get; private set; }
        public string Message { get; private set; }
        public string FormattedMessage { get; private set; }
        public DateTimeOffset TimeStamp { get; private set; }
        public Target Target { get; private set; }
    }
}