using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database;

namespace Vertica.Integration.Experiments
{
    public class CustomDb : Connection
    {
        public CustomDb(ConnectionString connectionString)
            : base(connectionString)
        {
        }
    }
}