using Microsoft.WindowsAzure.Storage.Blob;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage
{
	public interface IAzureBlobStorageClientFactory : IAzureBlobStorageClientFactory<DefaultConnection>
	{
	}

    public interface IAzureBlobStorageClientFactory<TConnection>
        where TConnection : Connection
    {
        CloudBlobClient Create();
    }
}