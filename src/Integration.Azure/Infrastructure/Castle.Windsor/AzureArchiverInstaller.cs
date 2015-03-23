using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Azure.Infrastructure.BlobStorage;
using Vertica.Integration.Azure.Infrastructure.BlobStorage.Archiving;
using Vertica.Integration.Infrastructure.Archiving;

namespace Vertica.Integration.Azure.Infrastructure.Castle.Windsor
{
    internal class AzureArchiverInstaller : IWindsorInstaller
    {
        private readonly string _connectionStringName;
        private readonly string _containerName;

        public AzureArchiverInstaller(string connectionStringName, string containerName)
        {
            if (String.IsNullOrWhiteSpace(connectionStringName)) throw new ArgumentException(@"Value cannot be null or empty.", "connectionStringName");
            if (String.IsNullOrWhiteSpace(containerName)) throw new ArgumentException(@"Value cannot be null or empty.", "containerName");

            _connectionStringName = connectionStringName;
            _containerName = containerName;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Install(
                new AzureBlobStorageInstaller<ArchiveConnection>(
                    new ArchiveConnection(_connectionStringName)));

            container.Register(
                Component
                    .For<IArchiver>()
                    .UsingFactoryMethod(kernel => new AzureBlobArchiver(
                        kernel.Resolve<IAzureBlobClientFactory<ArchiveConnection>>(), _containerName))
                    .IsDefault());
        }
    }
}