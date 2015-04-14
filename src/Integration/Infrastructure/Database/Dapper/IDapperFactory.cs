using System.Data;

namespace Vertica.Integration.Infrastructure.Database.Dapper
{
    public interface IDapperFactory : IDapperFactory<DefaultConnection>
	{
	}

    public interface IDapperFactory<TConnection>
		where TConnection : Connection
    {
        IDbConnection GetConnection();

        IDapperSession OpenSession();
	}
}