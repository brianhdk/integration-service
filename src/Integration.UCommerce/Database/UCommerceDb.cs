using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database;

namespace Vertica.Integration.UCommerce.Database
{
    public abstract class UCommerceDb : Connection
    {
        protected UCommerceDb(ConnectionString connectionString)
            : base(connectionString)
        {
        }
    }
}