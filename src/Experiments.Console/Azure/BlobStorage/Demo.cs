using Vertica.Integration;
using Vertica.Integration.Azure;
using Vertica.Integration.Azure.Infrastructure.BlobStorage;
using Vertica.Integration.Infrastructure;

namespace Experiments.Console.Azure.BlobStorage
{
    public static class Demo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .UseAzure(azure => azure
                    .BlobStorage(blobStorage => blobStorage
                        .DefaultConnection(ConnectionString.FromText("DefaultEndpointsProtocol=https;AccountName=xxxx;AccountKey=yyyy"))))))
            {
                var client = context.Resolve<IAzureBlobStorageClientFactory>().CreateBlobClient();
            }
        }
    }
}