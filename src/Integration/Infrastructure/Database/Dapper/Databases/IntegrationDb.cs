namespace Vertica.Integration.Infrastructure.Database.Dapper.Databases
{
    internal class IntegrationDb : DefaultConnection
	{
        public IntegrationDb(ConnectionString connectionString)
            : base(connectionString)
		{
		}
	}
}