using System;
using Castle.MicroKernel;
using Vertica.Integration.Perfion.Infrastructure.Client;

namespace Vertica.Integration.Perfion
{
    internal class PerfionClientFactory<TConnection> : IPerfionClientFactory<TConnection>
        where TConnection : Connection
    {
        private readonly Lazy<IPerfionClient> _client;

        public PerfionClientFactory(TConnection connection, IPerfionClientConfiguration configuration, IKernel kernel)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            _client = new Lazy<IPerfionClient>(() => new PerfionClientImpl(connection, configuration, kernel));
        }

        public IPerfionClient Client => _client.Value;
    }

    internal class PerfionClientFactory : IPerfionClientFactory
    {
        private readonly IPerfionClientFactory<DefaultConnection> _decoree;

        public PerfionClientFactory(IPerfionClientFactory<DefaultConnection> decoree)
        {
            if (decoree == null) throw new ArgumentNullException(nameof(decoree));

            _decoree = decoree;
        }

        public IPerfionClient Client => _decoree.Client;
    }
}