using System;

namespace Vertica.Integration.Elasticsearch
{
	public static class ElasticsearchExtensions
	{
		public static ApplicationConfiguration UseElasticsearch(this ApplicationConfiguration application, Action<ElasticsearchConfiguration> elasticsearch = null)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			return application.Extensibility(extensibility =>
			{
				ElasticsearchConfiguration configuration = extensibility.Register(() => new ElasticsearchConfiguration(application));

				elasticsearch?.Invoke(configuration);
			});
		}
	}
}