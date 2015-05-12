namespace Vertica.Integration.Infrastructure.Database.Dapper
{
    public abstract class DefaultConnection : Connection
	{
		protected DefaultConnection(ConnectionString connectionString)
			: base(connectionString)
		{
		}
	}
}