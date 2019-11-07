using System;
using Castle.MicroKernel;
using StackExchange.Redis;

namespace Vertica.Integration.Redis.Infrastructure.Client
{
    internal class RedisClientFactory<TConnection> : IRedisClientFactory<TConnection>
        where TConnection : Connection
    {
        private readonly Lazy<IConnectionMultiplexer> _client;

        public RedisClientFactory(TConnection connection, IKernel kernel)
        {
            _client = new Lazy<IConnectionMultiplexer>(() => connection.Connect(kernel));
        }

        public IConnectionMultiplexer Connection => _client.Value;

        public void Dispose()
        {
            if (_client.IsValueCreated)
                _client.Value.Dispose();
        }
    }

    internal class RedisClientFactory : IRedisClientFactory
    {
        private readonly IRedisClientFactory<DefaultConnection> _decoree;

        public RedisClientFactory(IRedisClientFactory<DefaultConnection> decoree)
        {
            if (decoree == null) throw new ArgumentNullException(nameof(decoree));

            _decoree = decoree;
        }

        public IConnectionMultiplexer Connection => _decoree.Connection;

        public void Dispose()
        {
            // we don't need to dispose, as IoC already knows about the decoree (it's by itself registred)
        }
    }
}