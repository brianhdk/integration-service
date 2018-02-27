using System;
using System.Threading;

namespace Vertica.Integration.Domain.LiteServer
{
    internal class HeartbeatLoggingWorker : IBackgroundWorker
    {
        private readonly IHeartbeatProvider[] _providers;
        private readonly TimeSpan _interval;

        public HeartbeatLoggingWorker(IHeartbeatProvider[] providers, TimeSpan interval)
        {
            if (providers == null) throw new ArgumentNullException(nameof(providers));
            if (providers.Length == 0) throw new ArgumentException(@"Value cannot be an empty collection.", nameof(providers));
            if (interval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval), interval, @"Value cannot be zero or negative.");

            _providers = providers;
            _interval = interval;
        }

        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            // TODO: Overvej om Housekeeping selv skal være en IHeartbeatProvider - 
            //  - og dermed returnere sine beskeder over servers der kører
            //  - derved kan vi _både_ outputte heartbeat-beskeder til konsollen - og samtidig persistere dem hvert X-interval

            foreach (IHeartbeatProvider provider in _providers)
            {

            }

            return context.Wait(_interval);
        }
        
        public override string ToString()
        {
            return nameof(HeartbeatLoggingWorker);
        }
    }
}