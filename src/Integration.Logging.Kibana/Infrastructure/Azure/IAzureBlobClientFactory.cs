using Microsoft.WindowsAzure.Storage.Blob;

namespace Vertica.Integration.Logging.Kibana.Infrastructure.Azure
{
    public interface IAzureBlobClientFactory
    {
        CloudBlobClient Create();
    }
}