namespace Vertica.Integration.Infrastructure.Database.Databases
{
    internal class IntegrationDb : DefaultConnection
	{
        public IntegrationDb(ConnectionString connectionString)
            : base(connectionString)
		{
		}
	}
}