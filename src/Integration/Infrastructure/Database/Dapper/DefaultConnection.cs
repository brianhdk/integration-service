namespace Vertica.Integration.Infrastructure.Database.Dapper
{
	public abstract class DefaultConnection : Connection
	{
		protected DefaultConnection(string connectionStringName)
			: base(connectionStringName)
		{
		}
	}
}