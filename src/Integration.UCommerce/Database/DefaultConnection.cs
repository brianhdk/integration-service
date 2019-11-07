using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.UCommerce.Database
{
    internal sealed class DefaultConnection : UCommerceDb
    {
        public DefaultConnection(ConnectionString connectionString)
            : base(connectionString)
        {
        }
    }
}