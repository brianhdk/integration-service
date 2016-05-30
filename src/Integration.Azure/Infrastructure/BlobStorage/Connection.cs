using System;
using Castle.MicroKernel;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Analytics;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage
{
	public abstract class Connection
	{
        protected Connection(ConnectionString connectionString)
		{
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

			ConnectionString = connectionString;
		}

        protected internal ConnectionString ConnectionString { get; }

		[Obsolete("Use CreateBlobClient method")]
		protected internal virtual CloudBlobClient Create(IKernel kernel)
		{
			return CreateBlobClient(kernel);
		}

		protected internal virtual CloudBlobClient CreateBlobClient(IKernel kernel)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));

			CloudStorageAccount account = GetAccount(kernel);
			CloudBlobClient client = account.CreateCloudBlobClient();

			return client;
		}

		protected internal virtual CloudQueueClient CreateQueueClient(IKernel kernel)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));

			CloudStorageAccount account = GetAccount(kernel);
			CloudQueueClient client = account.CreateCloudQueueClient();

			return client;
		}

		protected internal virtual CloudTableClient CreateTableClient(IKernel kernel)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));

			CloudStorageAccount account = GetAccount(kernel);
			CloudTableClient client = account.CreateCloudTableClient();

			return client;
		}

		protected internal virtual CloudAnalyticsClient CreateAnalyticsClient(IKernel kernel)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));

			CloudStorageAccount account = GetAccount(kernel);
			CloudAnalyticsClient client = account.CreateCloudAnalyticsClient();

			return client;
		}

		protected internal virtual CloudFileClient CreateFileClient(IKernel kernel)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));

			CloudStorageAccount account = GetAccount(kernel);
			CloudFileClient client = account.CreateCloudFileClient();

			return client;
		}

		protected CloudStorageAccount GetAccount(IKernel kernel)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));

			return CloudStorageAccount.Parse(ConnectionString);
		}
	}
}