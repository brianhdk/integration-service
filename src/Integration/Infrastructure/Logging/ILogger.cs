using System;

namespace Vertica.Integration.Infrastructure.Logging
{
    public interface ILogger
    {
        ErrorLog LogWarning(Target target, string message, params object[] args);

        ErrorLog LogError(Target target, string message, params object[] args);
		ErrorLog LogError(Exception exception, Target target = Target.Service);

        void LogEntry(LogEntry entry);

        IDisposable Disable();
    }
}