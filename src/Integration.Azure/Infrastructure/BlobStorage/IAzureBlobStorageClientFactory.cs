using System;
using Microsoft.WindowsAzure.Storage.Analytics;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage
{
	public interface IAzureBlobStorageClientFactory : IAzureBlobStorageClientFactory<DefaultConnection>
	{
	}

    public interface IAzureBlobStorageClientFactory<TConnection>
        where TConnection : Connection
    {
		[Obsolete("Use CreateBlobClient() method")]
        CloudBlobClient Create();

		CloudBlobClient CreateBlobClient();
		CloudQueueClient CreateQueueClient();
	    CloudTableClient CreateTableClient();
	    CloudAnalyticsClient CreateAnalyticsClient();
	    CloudFileClient CreateFileClient();
    }
}