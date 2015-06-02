using System;
using Vertica.Integration.Infrastructure.Configuration;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
    internal class TextFileLogger : Logger
    {
        private readonly TextFileLoggerConfiguration _configuration;

        public TextFileLogger(IConfigurationService configuration)
        {
            _configuration = configuration.Get<TextFileLoggerConfiguration>();
        }

        public override ErrorLog LogWarning(ITarget target, string message, params object[] args)
        {
            throw new NotImplementedException();
        }

        public override ErrorLog LogError(ITarget target, string message, params object[] args)
        {
            throw new NotImplementedException();
        }

        public override ErrorLog LogError(Exception exception, ITarget target = null)
        {
            throw new NotImplementedException();
        }

        public override void LogEntry(LogEntry entry)
        {
            throw new NotImplementedException();
        }
    }
}