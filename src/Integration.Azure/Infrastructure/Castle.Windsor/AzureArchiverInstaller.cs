using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Azure.Infrastructure.BlobStorage;
using Vertica.Integration.Azure.Infrastructure.BlobStorage.Archiving;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Archiving;

namespace Vertica.Integration.Azure.Infrastructure.Castle.Windsor
{
    internal class AzureArchiverInstaller : IWindsorInstaller
    {
        private readonly ConnectionString _connectionString;
        private readonly string _containerName;

        public AzureArchiverInstaller(ConnectionString connectionString, string containerName)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            if (String.IsNullOrWhiteSpace(containerName)) throw new ArgumentException(@"Value cannot be null or empty.", "containerName");

            _connectionString = connectionString;
            _containerName = containerName;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Install(
                new AzureBlobStorageInstaller<ArchiveConnection>(
                    new ArchiveConnection(_connectionString)));

            container.Register(
                Component
                    .For<IArchiver>()
                    .UsingFactoryMethod(kernel => new AzureBlobArchiver(
                        kernel.Resolve<IAzureBlobClientFactory<ArchiveConnection>>(), _containerName))
                    .IsDefault());
        }
    }
}