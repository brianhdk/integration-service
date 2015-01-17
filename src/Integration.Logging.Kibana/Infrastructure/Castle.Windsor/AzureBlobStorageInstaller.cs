using System;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Vertica.Integration.Logging.Kibana.Infrastructure.Azure;

namespace Vertica.Integration.Logging.Kibana.Infrastructure.Castle.Windsor
{
    public class AzureBlobStorageInstaller : IWindsorInstaller
    {
        private readonly string _connectionString;

        public AzureBlobStorageInstaller(string connectionString)
        {
            if (String.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException(@"Value cannot be null or empty.");

            _connectionString = connectionString;
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
                        CloudStorageAccount account = CloudStorageAccount.Parse(_connectionString);
                        CloudBlobClient client = account.CreateCloudBlobClient();

                        return client;
                    })
                    .LifestyleTransient());
        }
    }
}