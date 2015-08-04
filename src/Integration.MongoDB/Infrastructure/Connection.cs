using System;
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

		protected internal virtual MongoUrl MongoUrl
		{
			get { return new MongoUrl(ConnectionString); }
		}

	    protected internal virtual IMongoClient Create()
	    {
	        return new MongoClient(MongoUrl);
	    }
	}
}
