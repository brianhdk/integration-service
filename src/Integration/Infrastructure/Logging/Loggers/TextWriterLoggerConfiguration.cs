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
		    if (textWriter == null) throw new ArgumentNullException("textWriter");
		    if (log == null) throw new ArgumentNullException("log");

		    if (!_detailed)
		    {
			    textWriter.WriteLine(log.FormattedMessage);
			    return;
		    }

		    textWriter.WriteLine(String.Join(Environment.NewLine,
				log.MachineName,
				log.IdentityName,
				log.CommandLine,
				log.Severity,
				log.Target,
				log.TimeStamp,
				String.Empty,
				"---- BEGIN LOG",
				String.Empty,
				log.Message,
				String.Empty,
				log.FormattedMessage));
	    }

	    void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
	    {
		    container.RegisterInstance(this);
	    }
    }
}