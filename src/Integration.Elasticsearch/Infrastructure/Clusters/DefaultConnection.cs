using System;
using Castle.MicroKernel;
using Nest;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Elasticsearch.Infrastructure.Clusters
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

	    protected internal override IElasticClient Create(IKernel kernel)
	    {
            if (_connection != null)
	            return _connection.Create(kernel);

	        return base.Create(kernel);
	    }
	}
}