using System;
using System.IO;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
    public class TextWriterLoggerConfiguration
    {
	    private bool _detailed;

	    internal TextWriterLoggerConfiguration(LoggingConfiguration logging)
	    {
	        if (logging == null) throw new ArgumentNullException(nameof(logging));

	        Logging = logging.Change(l => l
                .Application
                    .Services(services => services
                        .Advanced(advanced => advanced
                            .Register(kernel => this))));
	    }

        public LoggingConfiguration Logging { get; }

        public TextWriterLoggerConfiguration Detailed()
	    {
		    _detailed = true;

		    return this;
	    }

	    internal void Write(TextWriter textWriter, ErrorLog log)
	    {
		    if (textWriter == null) throw new ArgumentNullException(nameof(textWriter));
		    if (log == null) throw new ArgumentNullException(nameof(log));

		    if (!_detailed)
		    {
			    textWriter.WriteLine(log.FormattedMessage);
			    return;
		    }

		    textWriter.WriteLine(string.Join(Environment.NewLine,
				log.MachineName,
				log.IdentityName,
				log.CommandLine,
				log.Severity,
				log.Target,
				log.TimeStamp,
				string.Empty,
				"---- BEGIN LOG",
				string.Empty,
				log.Message,
				string.Empty,
				log.FormattedMessage));
	    }
    }
}