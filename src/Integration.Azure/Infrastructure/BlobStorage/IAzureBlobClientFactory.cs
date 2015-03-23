using Microsoft.WindowsAzure.Storage.Blob;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage
{
    public interface IAzureBlobClientFactory<TConnection>
        where TConnection : Connection
    {
        CloudBlobClient Create();
    }
}