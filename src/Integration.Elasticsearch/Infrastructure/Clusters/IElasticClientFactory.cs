using Nest;

namespace Vertica.Integration.Elasticsearch.Infrastructure.Clusters
{
	public interface IElasticClientFactory : IElasticClientFactory<DefaultConnection>
	{
	}

    // ReSharper disable once UnusedTypeParameter
    public interface IElasticClientFactory<TConnection>
        where TConnection : Connection
    {
        IElasticClient Get();
    }
}