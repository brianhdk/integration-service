using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage
{
	public abstract class Connection
	{
        protected Connection(ConnectionString connectionString)
		{
            if (connectionString == null) throw new ArgumentNullException("connectionString");

			ConnectionString = connectionString;
		}

        protected internal ConnectionString ConnectionString { get; private set; }

		protected internal virtual CloudBlobClient Create()
		{
			CloudStorageAccount account = CloudStorageAccount.Parse(ConnectionString);
			CloudBlobClient client = account.CreateCloudBlobClient();

			return client;
		}
	}
}
