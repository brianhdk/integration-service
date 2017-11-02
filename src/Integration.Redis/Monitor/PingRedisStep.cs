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

        public override void Execute(ITaskExecutionContext<MonitorWorkItem> context)
        {
            try
            {
                IConnectionMultiplexer connection = _clientFactory.Connection;

                if (connection.IsConnected)
                    return;

                string status = connection.GetStatus();

                AddToWorkItem(context, status);
            }
            catch (Exception ex)
            {
                AddToWorkItem(context, ex.AggregateMessages());
            }
        }

        private static void AddToWorkItem(ITaskExecutionContext<MonitorWorkItem> context, string message)
        {
            context.Log.Message(message);

            context.WorkItem.Add(Time.UtcNow, "Redis", message);
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