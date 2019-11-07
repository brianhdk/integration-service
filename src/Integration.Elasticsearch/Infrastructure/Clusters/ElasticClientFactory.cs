using System;
using Castle.MicroKernel;
using Nest;

namespace Vertica.Integration.Elasticsearch.Infrastructure.Clusters
{
    internal class ElasticClientFactory<TConnection> : IElasticClientFactory<TConnection>
        where TConnection : Connection
    {
        private readonly Lazy<IElasticClient> _client;

	    public ElasticClientFactory(TConnection connection, IKernel kernel)
	    {
	        _client = new Lazy<IElasticClient>(() => connection.Create(kernel));
	    }

        public IElasticClient Get()
        {
            return _client.Value;
        }
    }

	internal class ElasticClientFactory : IElasticClientFactory
	{
		private readonly IElasticClientFactory<DefaultConnection> _decoree;

		public ElasticClientFactory(IElasticClientFactory<DefaultConnection> decoree)
		{
		    _decoree = decoree ?? throw new ArgumentNullException(nameof(decoree));
		}

        public IElasticClient Get()
        {
            return _decoree.Get();
        }
	}
}