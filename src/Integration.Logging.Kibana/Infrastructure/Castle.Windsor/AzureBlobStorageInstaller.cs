using System;
using System.Configuration;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Vertica.Integration.Logging.Kibana.Infrastructure.Azure;

namespace Vertica.Integration.Logging.Kibana.Infrastructure.Castle.Windsor
{
    internal class AzureBlobStorageInstaller : IWindsorInstaller
    {
        private readonly string _connectionStringName;

        public AzureBlobStorageInstaller(string connectionStringName)
        {
            if (String.IsNullOrWhiteSpace(connectionStringName)) throw new ArgumentException(@"Value cannot be null or empty.");

            _connectionStringName = connectionStringName;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component
                    .For<IAzureBlobClientFactory>()
                    .AsFactory());

            container.Register(
                Component
                    .For<CloudBlobClient>()
                    .UsingFactoryMethod(() =>
                    {
                        ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings[_connectionStringName];
                        CloudStorageAccount account = CloudStorageAccount.Parse(connectionString.ConnectionString);
                        CloudBlobClient client = account.CreateCloudBlobClient();

                        return client;
                    })
                    .LifestyleTransient());
        }
    }
}