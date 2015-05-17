using System;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage
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
