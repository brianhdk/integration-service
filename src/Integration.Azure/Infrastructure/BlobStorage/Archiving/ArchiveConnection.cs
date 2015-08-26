using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage.Archiving
{
	internal class ArchiveConnection : Connection
    {
        public ArchiveConnection(ConnectionString connectionString)
            : base(connectionString)
        {
        }
    }
}