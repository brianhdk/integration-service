using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.MongoDB.Infrastructure
{
    public sealed class DefaultConnection : Connection
    {
        public DefaultConnection(ConnectionString connectionString)
            : base(connectionString)
        {
        }
    }
}