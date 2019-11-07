using System;

namespace Vertica.Integration.Domain.LiteServer.Heartbeat.Logging
{
    public class HeartbeatLoggingConfiguration
    {
        private readonly InternalConfiguration _configuration;

        internal HeartbeatLoggingConfiguration(HeartbeatConfiguration heartbeat, InternalConfiguration configuration)
        {
            if (heartbeat == null) throw new ArgumentNullException(nameof(heartbeat));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _configuration = configuration;

            Heartbeat = heartbeat;

            // Default is logging every 5th minute
            Interval(TimeSpan.FromMinutes(5));
            
            // Default repository
            Repository<ConfigurationServiceBasedHeartbeatLoggingRepository>();
        }

        public HeartbeatConfiguration Heartbeat { get; }

        public HeartbeatLoggingConfiguration Interval(TimeSpan interval)
        {
            if (interval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval), interval, @"Value cannot be zero or negative.");

            _configuration.HeartbeatLoggingInterval = interval;

            return this;
        }

        public HeartbeatLoggingConfiguration Repository<TRepository>()
            where TRepository : class, IHeartbeatLoggingRepository
        {
            Heartbeat.LiteServer.Application.Services(services => services
                .Advanced(advanced => advanced.Register<IHeartbeatLoggingRepository, TRepository>()));

            return this;
        }

        public HeartbeatLoggingConfiguration Repository(IHeartbeatLoggingRepository repository)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));

            Heartbeat.LiteServer.Application.Services(services => services
                .Advanced(advanced => advanced.Register(kernel => repository)));

            return this;
        }
    }
}