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
            CommandLine = Environment.CommandLine;
            TimeStamp = Time.UtcNow;
        }

        public ErrorLog(Severity severity, string message, ITarget target)
            : this(severity, target)
        {
            Message = message;
            FormattedMessage = message;
        }

        public ErrorLog(Exception exception, ITarget target = null)
            : this(Severity.Error, target)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            Message = exception.DestructMessage();
            FormattedMessage = exception.GetFullStacktrace();
        }

        public string Id { get; internal set; }
        public string MachineName { get; }
        public string IdentityName { get; }
        public string CommandLine { get; }
        public Severity Severity { get; }
        public string Message { get; }
        public string FormattedMessage { get; }
        public DateTimeOffset TimeStamp { get; }
        public Target Target { get; }
    }
}