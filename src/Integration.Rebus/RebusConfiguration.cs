using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Rebus.Bus;
using Rebus.CastleWindsor;
using Rebus.Config;
using Rebus.Logging;

namespace Vertica.Integration.Rebus
{
    public class RebusConfiguration : IInitializable<IWindsorContainer>
    {
	    internal RebusConfiguration(ApplicationConfiguration application)
	    {
		    if (application == null) throw new ArgumentNullException("application");

		    application.Extensibility(extensibility => extensibility.Register(this));

		    HandlersConfiguration = new RebusHandlersConfiguration(application);
	    }

		private RebusHandlersConfiguration HandlersConfiguration { get; set; }

	    public RebusConfiguration Bus(Func<RebusConfigurer, RebusConfigurer> bus)
	    {
		    if (bus == null) throw new ArgumentNullException("bus");

		    BusConfiguration = bus;

		    return this;
	    }

	    public RebusConfiguration Handlers(Action<RebusHandlersConfiguration> handlers)
	    {
		    if (handlers == null) throw new ArgumentNullException("handlers");

		    handlers(HandlersConfiguration);

		    return this;
	    }

	    void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			container.Register(
			    Component.For<IBus>()
				    .UsingFactoryMethod(() =>
				    {
					    RebusConfigurer rebus =
						    Configure.With(new CastleWindsorContainerAdapter(container))
							    .Logging(logging => logging.ColoredConsole(LogLevel.Warn));

						// TODO: Look at this, whether to support logging to our own logging infrastructure (which we don't have)

					    if (BusConfiguration != null)
						    rebus = BusConfiguration(rebus);

					    return rebus.Start();
				    }));
		}

	    private Func<RebusConfigurer, RebusConfigurer> BusConfiguration { get; set; }
    }
}