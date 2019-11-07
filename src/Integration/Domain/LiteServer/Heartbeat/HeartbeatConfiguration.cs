using System;
using Vertica.Integration.Domain.LiteServer.Heartbeat.Logging;
using Vertica.Integration.Domain.LiteServer.Heartbeat.Providers;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Domain.LiteServer.Heartbeat
{
    public class HeartbeatConfiguration
    {
        private readonly InternalConfiguration _configuration;
        private readonly ScanAddRemoveInstaller<IHeartbeatProvider> _providers;

        internal HeartbeatConfiguration(LiteServerConfiguration liteServer, InternalConfiguration configuration)
        {
            if (liteServer == null) throw new ArgumentNullException(nameof(liteServer));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _providers = new ScanAddRemoveInstaller<IHeartbeatProvider>();
            _configuration = configuration;

            LiteServer = liteServer;

            LiteServer.Application 
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Install(_providers)));
            
            // Add default providers
            AddProvider<CurrentProcessHeartbeatProvider>();
            AddProvider<GarbageCollectorHeartbeatProvider>();
        }

        public LiteServerConfiguration LiteServer { get; }

        /// <summary>
        /// Enables logging of Heartbeats, default interval is every 15th minutes
        /// </summary>
        public HeartbeatConfiguration EnableLogging(Action<HeartbeatLoggingConfiguration> heartbeatLogging = null)
        {
            HeartbeatLoggingConfiguration instance = null;

            LiteServer.Application.Extensibility(extensibility =>
            {
                instance = extensibility.Register(() => new HeartbeatLoggingConfiguration(this, _configuration));
            });

            heartbeatLogging?.Invoke(instance);

            return this;
        }

        /// <summary>
        /// Scans the assembly of the defined <typeparamref name="T"></typeparamref> for public classes inheriting <see cref="IHeartbeatProvider"/> and/or <see cref="IBackgroundWorker"/>
        /// <para />
        /// </summary>
        /// <typeparam name="T">The type in which its assembly will be scanned.</typeparam>
        public HeartbeatConfiguration AddFromAssemblyOfThis<T>()
        {
            _providers.AddFromAssemblyOfThis<T>();

            return this;
        }

        /// <summary>
        /// Adds the specified <typeparamref name="TProvider"/>.
        /// </summary>
        /// <typeparam name="TProvider">Specifies the <see cref="IHeartbeatProvider"/> to be added.</typeparam>
        public HeartbeatConfiguration AddProvider<TProvider>()
            where TProvider : IHeartbeatProvider
        {
            _providers.Add<TProvider>();

            return this;
        }

        /// <summary>
        /// Skips the specified <typeparamref name="TProvider" />.
        /// </summary>
        /// <typeparam name="TProvider">Specifies the <see cref="IHeartbeatProvider"/> that will be skipped.</typeparam>
        public HeartbeatConfiguration RemoveProvider<TProvider>()
            where TProvider : IHeartbeatProvider
        {
            _providers.Remove<TProvider>();

            return this;
        }

        /// <summary>
        /// Clears all Providers.
        /// </summary>
        public HeartbeatConfiguration ClearProviders()
        {
            _providers.Clear();

            return this;
        }
    }
}