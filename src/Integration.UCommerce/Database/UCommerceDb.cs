using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database;

namespace Vertica.Integration.UCommerce.Database
{
    public class UCommerceDb : Connection
    {
        public UCommerceDb(ConnectionString connectionString)
            : base(connectionString)
        {
        }
    }
}