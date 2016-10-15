using System;
using Vertica.Integration.Infrastructure.Logging.Loggers;

namespace Vertica.Integration.Infrastructure.Logging
{
    public class LoggingConfiguration
    {
        internal LoggingConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application;

			// Make sure that the EventLoggerConfiguration has been registred as this will be used as a fallback.
			Application.Extensibility(extensibility => extensibility.Register(() => new EventLoggerConfiguration()));
        }

        public ApplicationConfiguration Application { get; }

        public LoggingConfiguration Use<T>() where T : Logger
        {
	        Application.Advanced(advanced => advanced.Register<ILogger, T>());
            
            return this;
        }

        public LoggingConfiguration Disable()
        {
            return Use<VoidLogger>();
        }

        public LoggingConfiguration EventLogger(Action<EventLoggerConfiguration> eventLogger = null)
        {
	        if (eventLogger != null)
		        Application.Extensibility(extensibility => eventLogger(extensibility.Get<EventLoggerConfiguration>()));

            return Use<EventLogger>();
        }

		public LoggingConfiguration TextFileLogger(Action<TextFileLoggerConfiguration> textFileLogger = null)
		{
			Application.Extensibility(extensibility =>
			{
				TextFileLoggerConfiguration configuration =
					extensibility.Register(() => new TextFileLoggerConfiguration());

                textFileLogger?.Invoke(configuration);
            });

			return Use<TextFileLogger>();
		}

	    public LoggingConfiguration TextWriter(Action<TextWriterLoggerConfiguration> textWriterLogger = null)
	    {
			Application.Extensibility(extensibility =>
			{
				TextWriterLoggerConfiguration configuration =
					extensibility.Register(() => new TextWriterLoggerConfiguration());

                textWriterLogger?.Invoke(configuration);
            });

			return Use<TextWriterLogger>();
	    }
    }
}