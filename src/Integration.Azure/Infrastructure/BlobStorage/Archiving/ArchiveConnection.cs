namespace Vertica.Integration.Azure.Infrastructure.BlobStorage.Archiving
{
    public class ArchiveConnection : Connection
    {
        public ArchiveConnection(string connectionStringName)
            : base(connectionStringName)
        {
        }
    }
}