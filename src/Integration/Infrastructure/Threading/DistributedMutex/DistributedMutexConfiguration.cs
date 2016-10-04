using System;

namespace Vertica.Integration.Infrastructure.Threading.DistributedMutex
{
    public class DistributedMutexConfiguration
    {
        public DistributedMutexConfiguration(TimeSpan waitTime)
        {
            WaitTime = waitTime;
        }

        public TimeSpan WaitTime { get; }
    }
}