using System;
using Castle.MicroKernel;
using Microsoft.WindowsAzure.Storage.Blob;

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
		    return _connection.Create(_kernel);
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