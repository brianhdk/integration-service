using System;
using System.Data;
using System.Data.SqlClient;

namespace Vertica.Integration.Infrastructure.Database
{
    internal class DbFactory<TConnection> : IDbFactory<TConnection> where TConnection : Connection
    {
        private readonly ConnectionString _connectionString;

        public DbFactory(ConnectionString connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");

            _connectionString = connectionString;
        }

        public IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
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
            return _decoree.OpenSession();
        }
    }
}