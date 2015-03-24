using System;
using System.Collections.Generic;
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

        public BeginArchive Create(string name, Action<CreatedArchive> onCreated)
        {
            if (onCreated == null) throw new ArgumentNullException("onCreated");
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");

            return new BeginArchive(stream =>
            {
                CloudBlobClient client = _factory.Create();

                CloudBlobContainer container = client.GetContainerReference(_containerName);
                container.CreateIfNotExists(BlobContainerPublicAccessType.Blob);

                string id = Guid.NewGuid().ToString("D");

                CloudBlockBlob blockBlob = LoadBlob(container, id);
                blockBlob.Metadata["Name"] = name;

                blockBlob.UploadFromStream(stream);

                onCreated(new CreatedArchive(id, String.Format("Download from {0}", blockBlob.Uri)));
            });
        }

        public Archive[] GetAll()
        {
            CloudBlobClient client = _factory.Create();

            CloudBlobContainer container = client.GetContainerReference(_containerName);

            if (!container.Exists())
                return new Archive[0];

            var list = new List<Archive>();

            foreach (CloudBlockBlob item in container
                .ListBlobs(blobListingDetails: BlobListingDetails.Metadata)
                .OfType<CloudBlockBlob>())
            {
                string name;
                if (item.Metadata.TryGetValue("Name", out name))
                {
                    list.Add(new Archive
                    {
                        Id = Path.GetFileNameWithoutExtension(item.Name),
                        Name = name,
                        ByteSize = item.Properties.Length,
                        Created = item.Properties.LastModified.GetValueOrDefault()
                    });                    
                }
            }

            return list.ToArray();
        }

        public byte[] Get(string id)
        {
            CloudBlobClient client = _factory.Create();

            CloudBlobContainer container = client.GetContainerReference(_containerName);
            container.CreateIfNotExists();

            CloudBlockBlob blob = LoadBlob(container, id);

            if (blob.Exists())
            {
                using (var memoryStream = new MemoryStream())
                {
                    blob.DownloadToStream(memoryStream);

                    return memoryStream.ToArray();
                }                
            }

            return null;
        }

        public int Delete(DateTimeOffset olderThan)
        {
            int deleted = 0;

            CloudBlobClient client = _factory.Create();

            CloudBlobContainer container = client.GetContainerReference(_containerName);

            if (container.Exists())
            {
                foreach (CloudBlockBlob item in container
                    .ListBlobs(blobListingDetails: BlobListingDetails.Metadata)
                    .OfType<CloudBlockBlob>())
                {
                    if (item.Properties.LastModified.GetValueOrDefault(olderThan) <= olderThan)
                    {
                        item.Delete();

                        deleted++;
                    }
                }                
            }

            return deleted;
        }

        private static CloudBlockBlob LoadBlob(CloudBlobContainer container, string id)
        {
            return container.GetBlockBlobReference(String.Format("{0}.zip", id));
        }
    }
}