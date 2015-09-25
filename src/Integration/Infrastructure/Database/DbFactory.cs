using System;
using System.Data;
using System.Data.SqlClient;
using Castle.MicroKernel;

namespace Vertica.Integration.Infrastructure.Database
{
	internal class DbFactory<TConnection> : IDbFactory<TConnection>
		where TConnection : Connection
    {
		private readonly TConnection _connection;
		private readonly IKernel _kernel;

		public DbFactory(TConnection connection, IKernel kernel)
	    {
		    if (connection == null) throw new ArgumentNullException("connection");
			if (kernel == null) throw new ArgumentNullException("kernel");

			_connection = connection;
			_kernel = kernel;
	    }

		public IDbConnection GetConnection()
	    {
		    return _connection.GetConnection(_kernel);
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