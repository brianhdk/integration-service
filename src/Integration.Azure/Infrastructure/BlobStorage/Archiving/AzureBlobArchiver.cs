using System;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;
using Vertica.Integration.Infrastructure.Archiving;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage.Archiving
{
    internal class AzureBlobArchiver : IArchiver
    {
        private readonly IAzureBlobClientFactory<ArchiveConnection> _factory;
        private readonly string _containerName;

        public AzureBlobArchiver(IAzureBlobClientFactory<ArchiveConnection> factory, string containerName)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (String.IsNullOrWhiteSpace(containerName)) throw new ArgumentException(@"Value cannot be null or empty.", "containerName");

            _factory = factory;
            _containerName = containerName;
        }

        public Archive Create(string name, Action<string> onCreated)
        {
            if (onCreated == null) throw new ArgumentNullException("onCreated");
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");

            return new Archive(stream =>
            {
                CloudBlobClient client = _factory.Create();

                CloudBlobContainer container = client.GetContainerReference(_containerName);
                container.CreateIfNotExists();

                string id = Guid.NewGuid().ToString("D");

                CloudBlockBlob blockBlob = container.GetBlockBlobReference(id);
                blockBlob.Metadata["Name"] = name;

                blockBlob.UploadFromStream(stream);

                onCreated(id);
            });
        }

        public SavedArchive[] GetAll()
        {
            throw new NotImplementedException();

            CloudBlobClient client = _factory.Create();

            CloudBlobContainer container = client.GetContainerReference(_containerName);
            container.CreateIfNotExists();

            foreach (CloudBlockBlob item in container.ListBlobs().OfType<CloudBlockBlob>())
            {
            }

            return new SavedArchive[0];
        }

        public byte[] Get(string id)
        {
            throw new NotImplementedException();

            CloudBlobClient client = _factory.Create();

            CloudBlobContainer container = client.GetContainerReference(_containerName);
            container.CreateIfNotExists();

            foreach (CloudBlockBlob item in container.ListBlobs().OfType<CloudBlockBlob>())
            {
                using (var memoryStream = new MemoryStream())
                {
                    //item.DownloadToStream(memoryStream);

                    //return memoryStream.ToArray();
                }
            }
        }

        public int Delete(DateTime olderThan)
        {
            throw new NotImplementedException();

            CloudBlobClient client = _factory.Create();

            CloudBlobContainer container = client.GetContainerReference(_containerName);
            container.CreateIfNotExists();

            foreach (CloudBlockBlob item in container.ListBlobs().OfType<CloudBlockBlob>())
            {
                //item.Delete();
            }
        }
    }
}