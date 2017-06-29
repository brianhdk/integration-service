using System;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Rebus.Bus;
using Rebus.Config;
using Vertica.Integration.Domain.LiteServer;

namespace Vertica.Integration.Rebus
{
    public class RebusConfiguration : IInitializable<IWindsorContainer>
    {
	    internal RebusConfiguration(ApplicationConfiguration application)
	    {
		    if (application == null) throw new ArgumentNullException(nameof(application));

            Application = application
                .Hosts(x => x.Host<RebusHost>())
                .Extensibility(extensibility =>
		        {
				    HandlersConfiguration = extensibility.Register(() => new RebusHandlersConfiguration(application));
		        });
	    }

		public ApplicationConfiguration Application { get; }

		private RebusHandlersConfiguration HandlersConfiguration { get; set; }

	    public RebusConfiguration Bus(Func<RebusConfigurer, IKernel, RebusConfigurer> bus)
	    {
		    if (bus == null) throw new ArgumentNullException(nameof(bus));

		    BusConfiguration = bus;

		    return this;
	    }

	    public RebusConfiguration Handlers(Action<RebusHandlersConfiguration> handlers)
	    {
		    if (handlers == null) throw new ArgumentNullException(nameof(handlers));

		    handlers(HandlersConfiguration);

		    return this;
	    }

        /// <summary>
        /// Adds Rebus to <see cref="ILiteServerFactory"/> allowing Rebus to run simultaneously with other servers.
        /// </summary>
        public RebusConfiguration AddToLiteServer()
        {
            Application.UseLiteServer(server => server.AddServer<RebusBackgroundServer>());

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialized(IWindsorContainer container)
		{
			container.Register(
			    Component.For<IBus>()
				    .UsingFactoryMethod(() =>
				    {
					    RebusConfigurer rebus =
						    Configure.With(new CastleWindsorContainerAdapter(container));

					    if (BusConfiguration != null)
						    rebus = BusConfiguration(rebus, container.Kernel);

                        // This will start Rebus - allowing one single instance of Rebus for this application.
					    return rebus.Start();
				    }));
		}

	    private Func<RebusConfigurer, IKernel, RebusConfigurer> BusConfiguration { get; set; }
    }
}