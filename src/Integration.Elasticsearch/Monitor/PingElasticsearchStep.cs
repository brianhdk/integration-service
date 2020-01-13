using System;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Elasticsearch.Infrastructure.Clusters;
using Vertica.Integration.Model;

namespace Vertica.Integration.Elasticsearch.Monitor
{
    [Obsolete("This step which can monitor Elasticsearch is no longer supported. You need to re-implement. This class will be removed.")]
    public class PingElasticsearchStep<TConnection> : Step<MonitorWorkItem>
        where TConnection : Connection
    {
        private readonly IElasticClientFactory<TConnection> _clientFactory;

        public PingElasticsearchStep(IElasticClientFactory<TConnection> connection)
        {
            _clientFactory = connection;
        }

        public override Execution ContinueWith(ITaskExecutionContext<MonitorWorkItem> context)
        {
            throw new NotSupportedException("This step which can monitor Elasticsearch is no longer supported. You need to re-implement. This class will be removed.");
        }

        public override void Execute(ITaskExecutionContext<MonitorWorkItem> context)
        {
            throw new NotSupportedException("This step which can monitor Elasticsearch is no longer supported. You need to re-implement. This class will be removed.");
        }

        public override string Description => $"Performs a Ping request to the Elasticsearch cluster for {_clientFactory}. [NOT SUPPORTED ANY MORE]";
    }

    [Obsolete("The step to monitor Elasticsearch is no longer supported. You need to re-implement.")]
    public class PingElasticsearchStep : PingElasticsearchStep<DefaultConnection>
    {
        public PingElasticsearchStep(IElasticClientFactory<DefaultConnection> connection)
            : base(connection)
        {
        }
    }
}