namespace Vertica.Integration.Infrastructure.Database.Dapper.Databases
{
	public class IntegrationDb : DefaultConnection
	{
        public IntegrationDb(ConnectionString connectionString)
            : base(connectionString)
		{
		}
	}
}