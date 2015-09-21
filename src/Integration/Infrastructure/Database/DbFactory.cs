using System;
using System.Data;
using System.Data.SqlClient;

namespace Vertica.Integration.Infrastructure.Database
{
	internal class DbFactory<TConnection> : IDbFactory<TConnection>
		where TConnection : Connection
    {
		private readonly TConnection _connection;

		public DbFactory(TConnection connection)
	    {
		    if (connection == null) throw new ArgumentNullException("connection");

		    _connection = connection;
	    }

		public IDbConnection GetConnection()
	    {
		    return _connection.GetConnection();
        }

        public IDbSession OpenSession()
        {
            return new DbSession(GetConnection());
        }
    }

    internal class DbFactory : IDbFactory
    {
	    private readonly IDbFactory<DefaultConnection> _decoree;

        public DbFactory(IDbFactory<DefaultConnection> decoree)
        {
	        if (decoree == null) throw new ArgumentNullException("decoree");

	        _decoree = decoree;
        }

		public IDbConnection GetConnection()
        {
            return _decoree.GetConnection();
        }

        public IDbSession OpenSession()
        {
            try
            {
                return _decoree.OpenSession();
            }
            catch (SqlException ex)
            {
                throw new IntegrationDbException(ex);
            }
        }
    }
}