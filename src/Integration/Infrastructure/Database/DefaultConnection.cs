namespace Vertica.Integration.Infrastructure.Database
{
    public abstract class DefaultConnection : Connection
	{
		protected DefaultConnection(ConnectionString connectionString)
			: base(connectionString)
		{
		}
	}
}