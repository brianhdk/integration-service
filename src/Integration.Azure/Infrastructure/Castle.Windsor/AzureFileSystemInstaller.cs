using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Azure.Infrastructure.BlobStorage;
using Vertica.Integration.Azure.Infrastructure.BlobStorage.IO;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.IO;

namespace Vertica.Integration.Azure.Infrastructure.Castle.Windsor
{
    internal class AzureFileSystemInstaller : IWindsorInstaller
    {
        private readonly ConnectionString _connectionString;
        private readonly string _containerName;

        public AzureFileSystemInstaller(ConnectionString connectionString, string containerName)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            if (String.IsNullOrWhiteSpace(containerName)) throw new ArgumentException(@"Value cannot be null or empty.", "containerName");

            _connectionString = connectionString;
            _containerName = containerName;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Install(
                new AzureBlobStorageInstaller<FileSystemConnection>(
                    new FileSystemConnection(_connectionString)));

            container.Register(
                Component
                    .For<IFileSystemService>()
                    .UsingFactoryMethod(kernel => new AzureFileSystemService(
                        kernel.Resolve<IAzureBlobStorageClientFactory<FileSystemConnection>>(), _containerName))
                    .IsDefault());
        }
    }
}