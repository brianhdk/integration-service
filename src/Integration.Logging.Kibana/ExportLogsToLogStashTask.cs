using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using Vertica.Integration.Logging.Kibana.Infrastructure.Azure;
using Vertica.Integration.Model;

namespace Vertica.Integration.Logging.Kibana
{
    public class ExportLogsToLogStashTask : Task<ExportLogsToLogStashWorkItem>
    {
        private readonly IAzureBlobClientFactory _factory;

        public ExportLogsToLogStashTask(IAzureBlobClientFactory factory, IEnumerable<IStep<ExportLogsToLogStashWorkItem>> steps)
            : base(steps)
        {
            _factory = factory;
        }

        public override string Description
        {
            get { return "Exports log files to LogStash."; }
        }

        public override string Schedule
        {
            get { return "Runs two times per day."; }
        }

        public override ExportLogsToLogStashWorkItem Start(Log log, params string[] arguments)
        {
            CloudBlobClient client = _factory.Create();

            CloudBlobContainer container = client.GetContainerReference("logs");
            container.CreateIfNotExists();

            Action<Stream, string> upload = (stream, name) =>
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(name);
                blockBlob.UploadFromStream(stream);

                log.Message("{0} uploaded.", name);
            };

            return new ExportLogsToLogStashWorkItem(upload);
        }
    }
}