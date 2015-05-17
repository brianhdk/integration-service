using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Azure.Infrastructure.BlobStorage;

namespace Vertica.Integration.Azure.Infrastructure.Castle.Windsor
{
    public class AzureBlobStorageInstaller<TConnection> : IWindsorInstaller
        where TConnection : Connection
    {
        private readonly TConnection _connection;

        public AzureBlobStorageInstaller(TConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            _connection = connection;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IAzureBlobClientFactory<TConnection>>()
                    .UsingFactoryMethod(() => new AzureBlobClientFactory<TConnection>(_connection.ConnectionString)));
        }
    }
}