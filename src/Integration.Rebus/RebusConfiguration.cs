using System;
using Castle.Windsor;
using Rebus.CastleWindsor;
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
			Container.RegisterInstance(this);
		}

	    private Func<RebusConfigurer, RebusConfigurer> BusConfiguration { get; set; }
	    private IWindsorContainer Container { get; set; }

	    internal Func<IDisposable> BusFactory
	    {
			get
			{
				if (Container == null)
					throw new InvalidOperationException("Container has not been initialized.");

				if (BusConfiguration == null)
					throw new InvalidOperationException("Bus has not been initialized.");

				return () =>
				{
					RebusConfigurer rebus = Configure.With(new CastleWindsorContainerAdapter(Container));

					return BusConfiguration(rebus).Start();
				};
			}
	    }
    }
}