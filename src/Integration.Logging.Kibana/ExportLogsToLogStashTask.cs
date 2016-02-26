using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using Vertica.Integration.Azure.Infrastructure.BlobStorage;
using Vertica.Integration.Logging.Kibana.Infrastructure;
using Vertica.Integration.Model;

namespace Vertica.Integration.Logging.Kibana
{
    public class ExportLogsToLogStashTask : Task<ExportLogsToLogStashWorkItem>
    {
        private readonly IAzureBlobStorageClientFactory<KibanaConnection> _factory;

        public ExportLogsToLogStashTask(IAzureBlobStorageClientFactory<KibanaConnection> factory, IEnumerable<IStep<ExportLogsToLogStashWorkItem>> steps)
            : base(steps)
        {
            _factory = factory;
        }

        public override string Description => "Exports log files to LogStash.";

	    public override ExportLogsToLogStashWorkItem Start(ITaskExecutionContext context)
        {
            CloudBlobClient client = _factory.Create();

            CloudBlobContainer container = client.GetContainerReference("logs");
            container.CreateIfNotExists();

            Action<Stream, string> upload = (stream, name) =>
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(name);
                blockBlob.UploadFromStream(stream);

                context.Log.Message("{0} uploaded.", name);
            };

            return new ExportLogsToLogStashWorkItem(upload);
        }
    }
}