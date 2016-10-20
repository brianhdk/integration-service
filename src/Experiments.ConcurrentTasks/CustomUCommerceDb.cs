using Vertica.Integration.Infrastructure;
using Vertica.Integration.UCommerce.Database;

namespace Experiments.ConcurrentTasks
{
    internal class CustomUCommerceDb : UCommerceDb
    {
        public CustomUCommerceDb()
            : base(ConnectionString.FromText("Server=.\\SQLExpress;Database=sc82-ucommerceSitecore_web;Trusted_Connection=True;"))
        {
        }
    }
}