using System;
using System.Data;
using System.Data.SqlClient;

namespace Vertica.Integration.Infrastructure.Database
{
	public abstract class Connection
	{
	    protected Connection(ConnectionString connectionString)
		{
	        if (connectionString == null) throw new ArgumentNullException("connectionString");

	        ConnectionString = connectionString;
		}

		protected ConnectionString ConnectionString { get; private set; }

		public virtual IDbConnection GetConnection()
		{
			return new SqlConnection(ConnectionString);
		}
	}
}