using System;
using StackExchange.Redis;

namespace Vertica.Integration.Redis.Infrastructure.Client
{
    public interface IRedisClientFactory<TConnection> : IDisposable
        where TConnection : Connection
    {
        IConnectionMultiplexer Connection { get; }
    }

    public interface IRedisClientFactory : IRedisClientFactory<DefaultConnection>
    {
    }
}