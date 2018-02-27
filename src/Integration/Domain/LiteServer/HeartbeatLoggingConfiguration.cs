using System;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Domain.LiteServer
{
    internal class HeartbeatLoggingConfiguration
    {
        private readonly InternalConfiguration _configuration;
        private readonly ScanAddRemoveInstaller<IHeartbeatProvider> _providers;

        internal HeartbeatLoggingConfiguration(HouseKeepingConfiguration houseKeeping, InternalConfiguration configuration)
        {
            if (houseKeeping == null) throw new ArgumentNullException(nameof(houseKeeping));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _providers = new ScanAddRemoveInstaller<IHeartbeatProvider>();
            _configuration = configuration;

            HouseKeeping = houseKeeping;

            HouseKeeping.LiteServer.Application 
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Install(_providers)));

            AddFromAssemblyOfThis<HeartbeatLoggingConfiguration>();

            // Default heartbeat interval
            Interval(TimeSpan.FromMinutes(15));
        }

        public HouseKeepingConfiguration HouseKeeping { get; }

        public HeartbeatLoggingConfiguration Interval(TimeSpan interval)
        {
            if (interval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval), interval, @"Value cannot be zero or negative.");

            _configuration.HeartbeatLoggingInterval = interval;

            return this;
        }

        /// <summary>
        /// Scans the assembly of the defined <typeparamref name="T"></typeparamref> for public classes inheriting <see cref="IHeartbeatProvider"/> and/or <see cref="IBackgroundWorker"/>
        /// <para />
        /// </summary>
        /// <typeparam name="T">The type in which its assembly will be scanned.</typeparam>
        public HeartbeatLoggingConfiguration AddFromAssemblyOfThis<T>()
        {
            _providers.AddFromAssemblyOfThis<T>();

            return this;
        }

        /// <summary>
        /// Adds the specified <typeparamref name="TProvider"/>.
        /// </summary>
        /// <typeparam name="TProvider">Specifies the <see cref="IHeartbeatProvider"/> to be added.</typeparam>
        public HeartbeatLoggingConfiguration AddProvider<TProvider>()
            where TProvider : IHeartbeatProvider
        {
            _providers.Add<TProvider>();

            return this;
        }

        /// <summary>
        /// Skips the specified <typeparamref name="TProvider" />.
        /// </summary>
        /// <typeparam name="TProvider">Specifies the <see cref="IHeartbeatProvider"/> that will be skipped.</typeparam>
        public HeartbeatLoggingConfiguration RemoveProvider<TProvider>()
            where TProvider : IHeartbeatProvider
        {
            _providers.Remove<TProvider>();

            return this;
        }

        /// <summary>
        /// Clears all providers.
        /// </summary>
        public HeartbeatLoggingConfiguration ClearProviders()
        {
            _providers.Clear();

            return this;
        }
    }
}