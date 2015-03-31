using Vertica.Integration.Azure.Infrastructure.BlobStorage;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Logging.Kibana.Infrastructure
{
    public class KibanaConnection : Connection
    {
        public KibanaConnection(ConnectionString connectionString)
            : base(connectionString)
        {
        }
    }
}