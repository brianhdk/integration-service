using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage
{
    internal class AzureBlobStorageClientFactory<TConnection> : IAzureBlobStorageClientFactory<TConnection>
        where TConnection : Connection
    {
	    private readonly TConnection _connection;

	    public AzureBlobStorageClientFactory(TConnection connection)
	    {
		    _connection = connection;
	    }

	    public CloudBlobClient Create()
	    {
		    return _connection.Create();
	    }
    }

	internal class AzureBlobStorageClientFactory : IAzureBlobStorageClientFactory
	{
		private readonly IAzureBlobStorageClientFactory<DefaultConnection> _decoree;

		public AzureBlobStorageClientFactory(IAzureBlobStorageClientFactory<DefaultConnection> decoree)
		{
			if (decoree == null) throw new ArgumentNullException("decoree");

			_decoree = decoree;
		}

		public CloudBlobClient Create()
		{
			return _decoree.Create();
		}
	}
}