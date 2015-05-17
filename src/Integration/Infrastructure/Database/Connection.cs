using System;

namespace Vertica.Integration.Infrastructure.Database
{
	public abstract class Connection
	{
	    protected Connection(ConnectionString connectionString)
		{
	        if (connectionString == null) throw new ArgumentNullException("connectionString");

	        ConnectionString = connectionString;
		}

	    internal ConnectionString ConnectionString { get; private set; }
	}
}
