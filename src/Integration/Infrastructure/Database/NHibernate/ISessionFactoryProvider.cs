using NHibernate;

namespace Vertica.Integration.Infrastructure.Database.NHibernate
{
	public interface ISessionFactoryProvider : ISessionFactoryProvider<DefaultConnection>
	{
	}

	public interface ISessionFactoryProvider<TConnection>
		where TConnection : Connection
	{
		ISessionFactory SessionFactory { get; }

		ICurrentSession CurrentSession { get; }
	}
}