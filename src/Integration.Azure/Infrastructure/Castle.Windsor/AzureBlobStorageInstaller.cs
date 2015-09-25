using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Azure.Infrastructure.BlobStorage;

namespace Vertica.Integration.Azure.Infrastructure.Castle.Windsor
{
	internal class AzureBlobStorageInstaller : AzureBlobStorageInstaller<DefaultConnection>
	{
		public AzureBlobStorageInstaller(DefaultConnection connection)
			: base(connection)
		{
		}

		public override void Install(IWindsorContainer container, IConfigurationStore store)
		{
			if (container.Kernel.HasComponent(typeof(IAzureBlobStorageClientFactory)))
				throw new InvalidOperationException("Only one DefaultConnection can be installed. Use the generic installer for additional instances.");

			base.Install(container, store);

			container.Register(
				Component.For<IAzureBlobStorageClientFactory>()
					.UsingFactoryMethod(kernel =>
						new AzureBlobStorageClientFactory(kernel.Resolve<IAzureBlobStorageClientFactory<DefaultConnection>>())));
		}
	}

	internal class AzureBlobStorageInstaller<TConnection> : IWindsorInstaller
        where TConnection : Connection
    {
        private readonly TConnection _connection;

        public AzureBlobStorageInstaller(TConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            _connection = connection;
        }

        public virtual void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IAzureBlobStorageClientFactory<TConnection>>()
                    .UsingFactoryMethod(kernel => new AzureBlobStorageClientFactory<TConnection>(_connection, kernel)));
        }
    }
}