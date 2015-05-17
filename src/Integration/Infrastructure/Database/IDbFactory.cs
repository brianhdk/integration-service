using System.Data;

namespace Vertica.Integration.Infrastructure.Database
{
    public interface IDbFactory : IDbFactory<DefaultConnection>
	{
	}

    public interface IDbFactory<TConnection>
		where TConnection : Connection
    {
        IDbConnection GetConnection();

        IDbSession OpenSession();
	}
}