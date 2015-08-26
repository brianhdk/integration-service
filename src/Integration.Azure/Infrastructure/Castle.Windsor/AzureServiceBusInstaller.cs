using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Azure.Infrastructure.ServiceBus;

namespace Vertica.Integration.Azure.Infrastructure.Castle.Windsor
{
	internal class AzureServiceBusInstaller : AzureServiceBusInstaller<DefaultConnection>
	{
		public AzureServiceBusInstaller(DefaultConnection connection)
			: base(connection)
		{
		}

		public override void Install(IWindsorContainer container, IConfigurationStore store)
		{
			if (container.Kernel.HasComponent(typeof(IAzureServiceBusClientFactory)))
				throw new InvalidOperationException("Only one DefaultConnection can be installed. Use the generic installer for additional instances.");

			base.Install(container, store);

			container.Register(
				Component.For<IAzureServiceBusClientFactory>()
					.UsingFactoryMethod(kernel =>
						new AzureServiceBusClientFactory(kernel.Resolve<IAzureServiceBusClientFactory<DefaultConnection>>())));
		}
	}

	internal class AzureServiceBusInstaller<TConnection> : IWindsorInstaller
        where TConnection : Connection
    {
        private readonly TConnection _connection;

		public AzureServiceBusInstaller(TConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            _connection = connection;
        }

        public virtual void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IAzureServiceBusClientFactory<TConnection>>()
                    .UsingFactoryMethod(() => new AzureServiceBusClientFactory<TConnection>(_connection)));
        }
    }
}