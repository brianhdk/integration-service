using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage
{
    internal class AzureBlobClientFactory<TConnection> : IAzureBlobClientFactory<TConnection>
        where TConnection : Connection
    {
        private readonly ConnectionString _connectionString;

        public AzureBlobClientFactory(ConnectionString connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");

            _connectionString = connectionString;
        }

        public CloudBlobClient Create()
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(_connectionString);
            CloudBlobClient client = account.CreateCloudBlobClient();

            return client;
        }
    }
}