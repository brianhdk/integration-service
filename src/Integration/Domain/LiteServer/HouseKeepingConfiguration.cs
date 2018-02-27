using System;

namespace Vertica.Integration.Domain.LiteServer
{
    public class HouseKeepingConfiguration
    {
        private readonly InternalConfiguration _configuration;

        internal HouseKeepingConfiguration(LiteServerConfiguration liteServer, InternalConfiguration configuration)
        {
            if (liteServer == null) throw new ArgumentNullException(nameof(liteServer));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            LiteServer = liteServer;
            _configuration = configuration;
        }

        public LiteServerConfiguration LiteServer { get; }

        /// <summary>
        /// Overrides how frequently the HouseKeeping thread should monitor background servers and workers.
        /// Default is every 5th second.
        /// </summary>
        public HouseKeepingConfiguration Interval(TimeSpan interval)
        {
            _configuration.HouseKeepingInterval = interval;

            return this;
        }

        /// <summary>
        /// Overrides the number of iterations HouseKeeping should do before outputting the current status of background servers and workers.
        /// Default is on every 12th iteration (= every minute, if the interval is 5 seconds).
        /// </summary>
        public HouseKeepingConfiguration OutputStatusOnNumberOfIterations(uint iteration)
        {
            _configuration.HouseKeepingOutputStatusOnNumberOfIterations = iteration;

            return this;
        }

        /// <summary>
        /// Enables the HouseKeeping to log heartbeat information.
        /// Defaults to every 15th minute.
        /// </summary>
        internal HouseKeepingConfiguration EnableHeartbeatLogging(Action<HeartbeatLoggingConfiguration> heartbeat = null)
        {
            HeartbeatLoggingConfiguration instance = null;

            LiteServer.Application.Extensibility(extensibility =>
            {
                instance = extensibility.Register(() => new HeartbeatLoggingConfiguration(this, _configuration));
            });

            heartbeat?.Invoke(instance);

            return this;
        }
    }
}