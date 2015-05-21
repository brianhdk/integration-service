using System;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
    public class VoidLogger : Logger
    {
        public override ErrorLog LogWarning(ITarget target, string message, params object[] args)
        {
            return new ErrorLog(Severity.Warning, String.Format(message, args), target);
        }

        public override ErrorLog LogError(ITarget target, string message, params object[] args)
        {
            return new ErrorLog(Severity.Error, String.Format(message, args), target);
        }

        public override ErrorLog LogError(Exception exception, ITarget target = null)
        {
            return new ErrorLog(exception, target);
        }

        public override void LogEntry(LogEntry entry)
        {
        }
    }
}