using System;
using System.IO;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
    public class TextWriterLoggerConfiguration : IInitializable<IWindsorContainer>
    {
	    private bool _detailed;

	    internal TextWriterLoggerConfiguration()
	    {
	    }

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

	    void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
	    {
		    container.RegisterInstance(this);
	    }
    }
}