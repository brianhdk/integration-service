using System;
using Vertica.Integration.Infrastructure.Logging.Loggers;

namespace Vertica.Integration.Infrastructure.Logging
{
    public class LoggingConfiguration
    {
        internal LoggingConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

			Application = application;
        }

        public ApplicationConfiguration Application { get; private set; }

        public LoggingConfiguration Use<T>() where T : Logger
        {
	        Application.Advanced(advanced => advanced.Register<ILogger, T>());
            
            return this;
        }

        public LoggingConfiguration Disable()
        {
            return Use<VoidLogger>();
        }

        public LoggingConfiguration EventLogger()
        {
            return Use<EventLogger>();
        }

		public LoggingConfiguration TextFileLogger(Action<TextFileLoggerConfiguration> textFileLogger = null)
		{
			Application.Extensibility(extensibility =>
			{
				TextFileLoggerConfiguration configuration =
					extensibility.Register(() => new TextFileLoggerConfiguration());

				if (textFileLogger != null)
					textFileLogger(configuration);
			});

			return Use<TextFileLogger>();
		}
    }
}