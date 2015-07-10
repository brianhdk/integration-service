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

            MongoUrl = new MongoUrl(connectionString);
		}

	    protected internal MongoUrl MongoUrl { get; private set; }

	    protected internal virtual IMongoClient Create()
	    {
	        return new MongoClient(MongoUrl);
	    }
	}
}
