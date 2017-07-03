using System;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Vertica.Integration.MongoDB.Commands;
using Vertica.Integration.MongoDB.Infrastructure;
using Vertica.Utilities;

namespace Vertica.Integration.MongoDB.Monitor
{
    public class PingMongoDbStep<TConnection> : Step<MonitorWorkItem>
        where TConnection : Connection
    {
        private readonly IMongoDbClientFactory<TConnection> _clientFactory;
        private readonly IPingCommand _ping;

        public PingMongoDbStep(IMongoDbClientFactory<TConnection> clientFactory, IPingCommand ping)
        {
            _ping = ping;
            _clientFactory = clientFactory;
        }

        public override void Execute(MonitorWorkItem workItem, ITaskExecutionContext context)
        {
            try
            {
                _ping.Execute(_clientFactory.Client, context.CancellationToken);
            }
            catch (AggregateException ex)
            {
                AddToWorkItem(workItem, context, ex.AggregateMessages());
            }
            catch (Exception ex)
            {
                AddToWorkItem(workItem, context, ex.AggregateMessages());
            }
        }

        private static void AddToWorkItem(MonitorWorkItem workItem, ITaskExecutionContext context, string message)
        {
            context.Log.Message(message);

            workItem.Add(Time.UtcNow, "MongoDb", message);
        }

        public override string Description => $"Performs a Ping request to the MongoDb cluster for {_clientFactory}.";
    }

    public class PingMongoDbStep : PingMongoDbStep<DefaultConnection>
    {
        public PingMongoDbStep(IMongoDbClientFactory<DefaultConnection> clientFactory, IPingCommand ping)
            : base(clientFactory, ping)
        {
        }
    }
}