﻿using System;
using Vertica.Integration.Infrastructure.IO;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage.IO
{
	internal class AzureFileSystemService : IFileSystemService
    {
        private readonly IAzureBlobStorageClientFactory<FileSystemConnection> _factory;
        private readonly string _containerName;

        public AzureFileSystemService(IAzureBlobStorageClientFactory<FileSystemConnection> factory, string containerName)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (String.IsNullOrWhiteSpace(containerName)) throw new ArgumentException(@"Value cannot be null or empty.", "containerName");

            _factory = factory;
            _containerName = containerName;
        }
    }
}