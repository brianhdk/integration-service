namespace Vertica.Integration.Infrastructure.Database.NHibernate
{
	public abstract class DefaultConnection : Connection
	{
		protected DefaultConnection(string connectionStringName)
			: base(connectionStringName)
		{
		}
	}
}