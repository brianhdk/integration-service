using System;
using Nest;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Elasticsearch.Infrastructure.Clusters;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Time = Vertica.Utilities.Time;

namespace Vertica.Integration.Elasticsearch.Monitor
{
    public class PingElasticsearchStep<TConnection> : Step<MonitorWorkItem>
        where TConnection : Connection
    {
        private readonly IElasticClientFactory<TConnection> _clientFactory;

        public PingElasticsearchStep(IElasticClientFactory<TConnection> connection)
        {
            _clientFactory = connection;
        }

        public override void Execute(MonitorWorkItem workItem, ITaskExecutionContext context)
        {
            try
            {
                IElasticClient client = _clientFactory.Get();

                var request = new PingRequest
                {
                    ErrorTrace = true
                };

                IPingResponse response = client.PingAsync(request, context.CancellationToken).Result;

                if (response.IsValid)
                    return;

                AddToWorkItem(workItem, context, response.DebugInformation);
            }
            catch (Exception ex)
            {
                AddToWorkItem(workItem, context, ex.AggregateMessages());
            }
        }

        private static void AddToWorkItem(MonitorWorkItem workItem, ITaskExecutionContext context, string message)
        {
            context.Log.Message(message);

            workItem.Add(Time.UtcNow, "Elasticsearch", message);
        }

        public override string Description => $"Performs a Ping request to the Elasticsearch cluster for {_clientFactory}.";
    }

    public class PingElasticsearchStep : PingElasticsearchStep<DefaultConnection>
    {
        public PingElasticsearchStep(IElasticClientFactory<DefaultConnection> connection)
            : base(connection)
        {
        }
    }
}