using System;
using Castle.Windsor;
using Rebus.Config;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Rebus
{
    public class RebusConfiguration : IInitializable<IWindsorContainer>
    {
	    internal RebusConfiguration(ApplicationConfiguration application)
	    {
		    if (application == null) throw new ArgumentNullException("application");

		    application.RegisterInitialization(this);
	    }

	    public RebusConfiguration Bus(Func<RebusConfigurer, RebusConfigurer> bus)
	    {
		    if (bus == null) throw new ArgumentNullException("bus");

		    BusConfiguration = bus;

		    return this;
	    }

	    void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			Container = container;
			container.RegisterInstance(this);
		}

	    internal Func<RebusConfigurer, RebusConfigurer> BusConfiguration { get; set; }
	    internal IWindsorContainer Container { get; set; }
    }
}