using System;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
    public class EventLoggerConfiguration : IInitializable<IWindsorContainer>
    {
	    internal EventLoggerConfiguration()
	    {
	    }

	    public EventLoggerConfiguration Source(string name)
	    {
		    if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));

			SourceName = name;

		    return this;
	    }

	    internal string SourceName { get; private set; }

		void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			container.RegisterInstance(this);
		}
    }
}