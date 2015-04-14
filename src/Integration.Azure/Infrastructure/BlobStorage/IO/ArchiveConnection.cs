using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage.IO
{
    public class FileSystemConnection : Connection
    {
        public FileSystemConnection(ConnectionString connectionString)
            : base(connectionString)
        {
        }
    }
}