using System;
using Vertica.Integration.Elasticsearch.Infrastructure.Clusters;

namespace Vertica.Integration.Elasticsearch
{
	public class ElasticsearchConfiguration
	{
        private readonly ElasticClustersConfiguration _clusters;

        internal ElasticsearchConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

		    Application = application;

            _clusters = new ElasticClustersConfiguration(this);
            Application.Extensibility(extensibility => extensibility.Register(() => _clusters));
        }

	    public ApplicationConfiguration Application { get; }

	    public ElasticsearchConfiguration Change(Action<ElasticsearchConfiguration> change)
		{
			change?.Invoke(this);

			return this;
		}

        public ElasticsearchConfiguration Clusters(Action<ElasticClustersConfiguration> clusters)
        {
            if (clusters == null) throw new ArgumentNullException(nameof(clusters));

            clusters(_clusters);

            return this;
        }
    }
}