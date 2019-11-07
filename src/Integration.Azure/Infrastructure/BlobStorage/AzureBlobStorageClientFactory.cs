using System;
using Castle.MicroKernel;
using Microsoft.WindowsAzure.Storage.Analytics;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage
{
    internal class AzureBlobStorageClientFactory<TConnection> : IAzureBlobStorageClientFactory<TConnection>
        where TConnection : Connection
    {
	    private readonly TConnection _connection;
	    private readonly IKernel _kernel;

	    public AzureBlobStorageClientFactory(TConnection connection, IKernel kernel)
	    {
		    _connection = connection;
		    _kernel = kernel;
	    }

	    public CloudBlobClient Create()
	    {
		    return CreateBlobClient();
	    }

	    public CloudBlobClient CreateBlobClient()
	    {
		    return _connection.CreateBlobClient(_kernel);
	    }

	    public CloudQueueClient CreateQueueClient()
	    {
		    return _connection.CreateQueueClient(_kernel);
	    }

	    public CloudTableClient CreateTableClient()
	    {
		    return _connection.CreateTableClient(_kernel);
	    }

	    public CloudAnalyticsClient CreateAnalyticsClient()
	    {
		    return _connection.CreateAnalyticsClient(_kernel);
	    }

	    public CloudFileClient CreateFileClient()
	    {
		    return _connection.CreateFileClient(_kernel);
	    }
    }

	internal class AzureBlobStorageClientFactory : IAzureBlobStorageClientFactory
	{
		private readonly IAzureBlobStorageClientFactory<DefaultConnection> _decoree;

		public AzureBlobStorageClientFactory(IAzureBlobStorageClientFactory<DefaultConnection> decoree)
		{
			if (decoree == null) throw new ArgumentNullException(nameof(decoree));

			_decoree = decoree;
		}

		public CloudBlobClient Create()
		{
			return CreateBlobClient();
		}

		public CloudBlobClient CreateBlobClient()
		{
			return _decoree.CreateBlobClient();
		}

		public CloudQueueClient CreateQueueClient()
		{
			return _decoree.CreateQueueClient();
		}

		public CloudTableClient CreateTableClient()
		{
			return _decoree.CreateTableClient();
		}

		public CloudAnalyticsClient CreateAnalyticsClient()
		{
			return _decoree.CreateAnalyticsClient();
		}

		public CloudFileClient CreateFileClient()
		{
			return _decoree.CreateFileClient();
		}
	}
}