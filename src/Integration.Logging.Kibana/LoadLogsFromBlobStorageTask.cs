using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;
using Vertica.Integration.Azure.Infrastructure.BlobStorage;
using Vertica.Integration.Logging.Kibana.Infrastructure;
using Vertica.Integration.Model;

namespace Vertica.Integration.Logging.Kibana
{
    public class LoadLogsFromBlobStorageTask : Task
    {
        private readonly IAzureBlobStorageClientFactory<KibanaConnection> _factory;

        public LoadLogsFromBlobStorageTask(IAzureBlobStorageClientFactory<KibanaConnection> factory)
        {
            _factory = factory;
        }

        public override string Description
        {
            get { return "Loads Log files from BlobStorage in Azure."; }
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            CloudBlobClient client = _factory.Create();

            CloudBlobContainer container = client.GetContainerReference("logs");
            container.CreateIfNotExists();

            // todo: configurable
            var directory = new DirectoryInfo(@"c:\tmp\azure-logs");

            if (!directory.Exists)
                directory.Create();

            foreach (CloudBlockBlob item in container.ListBlobs().OfType<CloudBlockBlob>())
            {
                using (FileStream fileStream = File.OpenWrite(Path.Combine(directory.FullName, item.Name)))
                {
                    item.DownloadToStream(fileStream);
                }

                item.Delete();
            }
        }
    }
}