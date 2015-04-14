using System;
using Vertica.Integration.Infrastructure.IO;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage.IO
{
    public class AzureFileSystemService : IFileSystemService
    {
        private readonly IAzureBlobClientFactory<FileSystemConnection> _factory;
        private readonly string _containerName;

        public AzureFileSystemService(IAzureBlobClientFactory<FileSystemConnection> factory, string containerName)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (String.IsNullOrWhiteSpace(containerName)) throw new ArgumentException(@"Value cannot be null or empty.", "containerName");

            _factory = factory;
            _containerName = containerName;
        }
    }
}