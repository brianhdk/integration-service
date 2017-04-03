using System;
using Castle.MicroKernel;
using StackExchange.Redis;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Redis.Infrastructure.Client
{
	public abstract class Connection
	{
        protected Connection(ConnectionString connectionString)
		{
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

			ConnectionString = connectionString;
		}

        protected internal ConnectionString ConnectionString { get; }

		protected internal virtual IConnectionMultiplexer Connect(IKernel kernel)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));

		    var client = ConnectionMultiplexer.Connect(ConnectionString);

		    return client;
		}
	}
}