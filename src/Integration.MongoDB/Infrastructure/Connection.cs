using System;
using Castle.MicroKernel;
using MongoDB.Driver;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.MongoDB.Infrastructure
{
	public abstract class Connection
	{
		protected Connection(ConnectionString connectionString)
		{
			if (connectionString == null) throw new ArgumentNullException("connectionString");

			ConnectionString = connectionString;
		}

		protected internal ConnectionString ConnectionString { get; private set; }

		protected internal virtual MongoUrl CreateMongoUrl(IKernel kernel)
		{
			if (kernel == null) throw new ArgumentNullException("kernel");

			return new MongoUrl(ConnectionString);
		}

		protected internal virtual IMongoClient Create(IKernel kernel)
	    {
		    if (kernel == null) throw new ArgumentNullException("kernel");

			return new MongoClient(CreateMongoUrl(kernel));
	    }
	}
}
