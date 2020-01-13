using System;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Elasticsearch.Infrastructure.Clusters;
using Vertica.Integration.Elasticsearch.Monitor;
using Vertica.Integration.Model;

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

		[Obsolete("The step to monitor Elasticsearch is no longer supported. You need to re-implement.")]
		public static TaskConfiguration<MonitorWorkItem> IncludeElasticsearch(this TaskConfiguration<MonitorWorkItem> task)
	    {
	        if (task == null) throw new ArgumentNullException(nameof(task));

	        return task.Step<PingElasticsearchStep>();
	    }

        [Obsolete("The step to monitor Elasticsearch is no longer supported. You need to re-implement.")]
		public static TaskConfiguration<MonitorWorkItem> IncludeElasticsearch<TConnection>(this TaskConfiguration<MonitorWorkItem> task)
	        where TConnection : Connection
	    {
	        return task.Step<PingElasticsearchStep<TConnection>>();
	    }
    }
}