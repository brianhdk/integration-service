using System;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
    public static class TextFileLoggerExtensions
    {
        public static LoggingConfiguration UseTextFileLogger(this LoggingConfiguration logging, Action<TextFileLoggerConfiguration> textFileLogger = null)
        {
            if (logging == null) throw new ArgumentNullException("logging");

            var configuration = new TextFileLoggerConfiguration();

            if (textFileLogger != null)
                textFileLogger(configuration);

            logging.Application.RegisterDependency(configuration);
            logging.Use<TextFileLogger>();

            return logging;
        }
    }
}