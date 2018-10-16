using System;
using System.Linq;
using Castle.MicroKernel;
using Elasticsearch.Net;
using Nest;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Elasticsearch.Infrastructure.Clusters
{
	public abstract class Connection
	{
        protected Connection(ConnectionString connectionString)
		{
		    ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
		}

        protected internal ConnectionString ConnectionString { get; }

		protected internal virtual IElasticClient Create(IKernel kernel)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));

		    ConnectionSettings settings = GetSettings(GetConnectionPool(kernel), kernel);

            var client = new ElasticClient(settings);

		    return client;
		}

	    protected virtual ConnectionSettings GetSettings(IConnectionPool pool, IKernel kernel)
	    {
	        var settings = new ConnectionSettings(pool);

	        return settings;
	    }

	    protected virtual IConnectionPool GetConnectionPool(IKernel kernel)
	    {
	        if (kernel == null) throw new ArgumentNullException(nameof(kernel));

	        Uri[] nodes = GetNodeUrls(kernel);

	        if (nodes.Length == 1)
	            return new SingleNodeConnectionPool(nodes[0]);

	        return new StaticConnectionPool(nodes);
	    }

	    protected virtual Uri[] GetNodeUrls(IKernel kernel)
	    {
	        if (kernel == null) throw new ArgumentNullException(nameof(kernel));

	        Uri[] nodes = ConnectionString
	            .ToString()
	            .Split(new[] { "|", ",", ";" }, StringSplitOptions.RemoveEmptyEntries)
	            .Select(x => new Uri(x.Trim()))
	            .Distinct()
	            .ToArray();

            if (nodes.Length == 0)
	            throw new InvalidOperationException("One or more Urls is required.");

	        return nodes;
	    }
	}
}