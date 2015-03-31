using System;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
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
                Component.For<CloudBlobClient>()
                    .Named(_connection.CloudBlobClient)
                    .UsingFactoryMethod(kernel =>
                    {
                        CloudStorageAccount account = CloudStorageAccount.Parse(_connection.ConnectionStringInternal);
                        CloudBlobClient client = account.CreateCloudBlobClient();

                        return client;                        
                    })
                    .LifeStyle.Transient);

            container.Register(
                Component.For<AzureBlobStorageSelector<TConnection>>()
                    .Named(_connection.SelectorName)
                    .UsingFactoryMethod(() => new AzureBlobStorageSelector<TConnection>(_connection)));

            container.Register(
                Component.For<IAzureBlobClientFactory<TConnection>>()
                    .Named(_connection.FactoryName)
                    .AsFactory(cfg => cfg.SelectedWith(_connection.SelectorName)));
        }
    }
}