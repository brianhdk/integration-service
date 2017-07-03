using System;

namespace Vertica.Integration.Domain.LiteServer
{
    internal class InternalConfiguration
    {
        private TimeSpan _shutdownTimeout;
        private TimeSpan _houseKeepingInterval;

        public InternalConfiguration()
        {
            OnStartup = new StartupActions();
            OnShutdown = new ShutdownActions();
            ShutdownTimeout = TimeSpan.FromSeconds(5);
            HouseKeepingInterval = TimeSpan.FromSeconds(5);
            HouseKeepingOutputStatusOnNumberOfIterations = 1;
        }

        public StartupActions OnStartup { get; }
        public ShutdownActions OnShutdown { get; }

        public TimeSpan ShutdownTimeout
        {
            get { return _shutdownTimeout; }
            set
            {
                if (value < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(value), @"Value cannot be negative.");

                _shutdownTimeout = value;
            }
        }

        public TimeSpan HouseKeepingInterval
        {
            get { return _houseKeepingInterval; }
            set
            {
                if (value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(value), value, @"Value cannot be zero or negative.");

                _houseKeepingInterval = value;
            }
        }

        public uint HouseKeepingOutputStatusOnNumberOfIterations { get; set; }
    }
}