using System;
using Vertica.Integration.Infrastructure.Logging.Loggers;

namespace Vertica.Integration.Infrastructure.Logging
{
    public class LoggingConfiguration
    {
        private readonly EventLoggerConfiguration _eventLogger;

        internal LoggingConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            Application = application;

            _eventLogger = new EventLoggerConfiguration(this);

			Application.Extensibility(extensibility => extensibility.Register(() => _eventLogger));
        }

        public ApplicationConfiguration Application { get; }
        
        public LoggingConfiguration Change(Action<LoggingConfiguration> change)
        {
            change?.Invoke(this);

            return this;
        }

        public LoggingConfiguration Use<TLogger>() 
            where TLogger : Logger
        {
	        Application.Services(services => services
                .Advanced(advanced => advanced
                    .Register<ILogger, TLogger>()));
            
            return this;
        }

        public LoggingConfiguration Disable()
        {
            return Use<VoidLogger>();
        }

        public LoggingConfiguration EventLogger(Action<EventLoggerConfiguration> eventLogger = null)
        {
            eventLogger?.Invoke(_eventLogger);

            return Use<EventLogger>();
        }

		public LoggingConfiguration TextFileLogger(Action<TextFileLoggerConfiguration> textFileLogger = null)
		{
			Application.Extensibility(extensibility =>
			{
				TextFileLoggerConfiguration configuration =
					extensibility.Register(() => new TextFileLoggerConfiguration(this));

                textFileLogger?.Invoke(configuration);
            });

			return Use<TextFileLogger>();
		}

	    public LoggingConfiguration TextWriter(Action<TextWriterLoggerConfiguration> textWriterLogger = null)
	    {
			Application.Extensibility(extensibility =>
			{
				TextWriterLoggerConfiguration configuration =
					extensibility.Register(() => new TextWriterLoggerConfiguration(this));

                textWriterLogger?.Invoke(configuration);
            });

			return Use<TextWriterLogger>();
	    }
    }
}