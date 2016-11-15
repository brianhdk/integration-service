using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Vertica.Integration.Elasticsearch.Infrastructure.Clusters.Castle.Windsor;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Elasticsearch.Infrastructure.Clusters
{
	public class ElasticClustersConfiguration : IInitializable<ApplicationConfiguration>
	{
		private IWindsorInstaller _defaultConnection;
		private readonly List<IWindsorInstaller> _connections;

		internal ElasticClustersConfiguration(ElasticsearchConfiguration elasticsearch)
        {
            if (elasticsearch == null) throw new ArgumentNullException(nameof(elasticsearch));

		    _connections = new List<IWindsorInstaller>();

		    Elasticsearch = elasticsearch;
        }

        public ElasticsearchConfiguration Elasticsearch { get; }

		public ElasticClustersConfiguration DefaultConnection(ConnectionString connectionString)
		{
			if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

			_defaultConnection = new ElasticClusterInstaller(new DefaultConnection(connectionString));

			return this;
		}

		public ElasticClustersConfiguration DefaultConnection(Connection connection)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_defaultConnection = new ElasticClusterInstaller(new DefaultConnection(connection));

			return this;
		}

		public ElasticClustersConfiguration AddConnection<TConnection>(TConnection connection)
			where TConnection : Connection
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_connections.Add(new ElasticClusterInstaller<TConnection>(connection));

			return this;
		}

        void IInitializable<ApplicationConfiguration>.Initialized(ApplicationConfiguration application)
        {
            application.Services(services => services.Advanced(advanced =>
            {
                if (_defaultConnection == null)
                    DefaultConnection(ConnectionString.FromName("Elasticsearch"));

                if (_defaultConnection != null)
                    advanced.Install(_defaultConnection);

                advanced.Install(_connections.ToArray());
            }));
        }
    }
}