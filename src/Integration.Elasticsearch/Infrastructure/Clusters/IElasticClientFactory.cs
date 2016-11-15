using Nest;

namespace Vertica.Integration.Elasticsearch.Infrastructure.Clusters
{
	public interface IElasticClientFactory : IElasticClientFactory<DefaultConnection>
	{
	}

    public interface IElasticClientFactory<TConnection>
        where TConnection : Connection
    {
        IElasticClient Get();
    }
}