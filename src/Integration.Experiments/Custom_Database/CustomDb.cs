using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Dapper;

namespace Vertica.Integration.Experiments.Custom_Database
{
    public class CustomDb : Connection
    {
        public CustomDb() : base(ConnectionString.FromName("CustomDb"))
        {
        }
    }
}