using Vertica.Integration.Azure.Infrastructure.BlobStorage;

namespace Vertica.Integration.Logging.Kibana.Infrastructure
{
    public class KibanaConnection : Connection
    {
        public KibanaConnection(string connectionStringName)
            : base(connectionStringName)
        {
        }
    }
}