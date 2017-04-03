using System;
using Castle.MicroKernel;
using StackExchange.Redis;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Redis.Infrastructure.Client
{
	public sealed class DefaultConnection : Connection
	{
		private readonly Connection _connection;

		internal DefaultConnection(ConnectionString connectionString)
			: base(connectionString)
		{
		}

		internal DefaultConnection(Connection connection)
			: base(connection.ConnectionString)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_connection = connection;
		}

	    protected internal override IConnectionMultiplexer Connect(IKernel kernel)
	    {
            if (_connection != null)
	            return _connection.Connect(kernel);

	        return base.Connect(kernel);
	    }
	}
}