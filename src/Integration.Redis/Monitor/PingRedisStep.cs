using System;
using StackExchange.Redis;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Vertica.Integration.Redis.Infrastructure.Client;
using Vertica.Utilities;

namespace Vertica.Integration.Redis.Monitor
{
    public class PingRedisStep<TConnection> : Step<MonitorWorkItem>
        where TConnection : Connection
    {
        private readonly IRedisClientFactory<TConnection> _clientFactory;

        public PingRedisStep(IRedisClientFactory<TConnection> clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public override void Execute(MonitorWorkItem workItem, ITaskExecutionContext context)
        {
            try
            {
                IConnectionMultiplexer connection = _clientFactory.Connection;

                if (connection.IsConnected)
                    return;

                string status = connection.GetStatus();

                AddToWorkItem(workItem, context, status);
            }
            catch (Exception ex)
            {
                AddToWorkItem(workItem, context, ex.AggregateMessages());
            }
        }

        private static void AddToWorkItem(MonitorWorkItem workItem, ITaskExecutionContext context, string message)
        {
            context.Log.Message(message);

            workItem.Add(Time.UtcNow, "Redis", message);
        }

        public override string Description => $"Performs a Ping request to the Redis cluster for {_clientFactory}.";
    }

    public class PingRedisStep : PingRedisStep<DefaultConnection>
    {
        public PingRedisStep(IRedisClientFactory<DefaultConnection> clientFactory)
            : base(clientFactory)
        {
        }
    }
}