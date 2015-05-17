using System;

namespace Vertica.Integration.Infrastructure.Logging
{
    public interface ILogger
    {
        ErrorLog LogWarning(ITarget target, string message, params object[] args);

        ErrorLog LogError(ITarget target, string message, params object[] args);
		ErrorLog LogError(Exception exception, ITarget target = null);

        void LogEntry(LogEntry entry);

        IDisposable Disable();
    }
}