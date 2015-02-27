using System.Data;

namespace Vertica.Integration.Infrastructure.Database.Dapper
{
    public interface IDapperProvider : IDapperProvider<DefaultConnection>
	{
	}

    public interface IDapperProvider<TConnection>
		where TConnection : Connection
    {
        IDbConnection GetConnection();

        IDapperSession OpenSession();
	}
}