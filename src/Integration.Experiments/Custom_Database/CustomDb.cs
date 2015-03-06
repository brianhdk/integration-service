using Vertica.Integration.Infrastructure.Database.Dapper;

namespace Vertica.Integration.Experiments.Custom_Database
{
    public class CustomDb : Connection
    {
        public CustomDb() : base("CustomDb")
        {
        }
    }
}