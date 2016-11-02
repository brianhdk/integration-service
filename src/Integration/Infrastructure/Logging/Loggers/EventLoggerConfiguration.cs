using System;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
    public class EventLoggerConfiguration
    {
        internal EventLoggerConfiguration(LoggingConfiguration logging)
        {
            if (logging == null) throw new ArgumentNullException(nameof(logging));

            Logging = logging.Change(l => l
                .Application
                    .Services(services => services
                        .Advanced(advanced => advanced
                            .Register(kernel => this))));
        }

        public LoggingConfiguration Logging { get; }

        public EventLoggerConfiguration Source(string name)
	    {
		    if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));

			SourceName = name;

		    return this;
	    }

	    internal string SourceName { get; private set; }
    }
}