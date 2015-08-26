using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage.IO
{
	internal class FileSystemConnection : Connection
    {
        public FileSystemConnection(ConnectionString connectionString)
            : base(connectionString)
        {
        }
    }
}