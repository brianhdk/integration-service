using System;
using System.Threading;
using Vertica.Utilities;

namespace Vertica.Integration.Domain.LiteServer.Heartbeat.Logging
{
    internal class HeartbeatLoggingWorker : IBackgroundWorker
    {
        private readonly IHeartbeatProvider[] _providers;
        private readonly TimeSpan _interval;
        private readonly IHeartbeatLoggingRepository _repository;

        public HeartbeatLoggingWorker(IHeartbeatProvider[] providers, TimeSpan interval, IHeartbeatLoggingRepository repository)
        {
            if (providers == null) throw new ArgumentNullException(nameof(providers));
            if (providers.Length == 0) throw new ArgumentException(@"Value cannot be an empty collection.", nameof(providers));
            if (interval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval), interval, @"Value cannot be zero or negative.");
            if (repository == null) throw new ArgumentNullException(nameof(repository));

            _providers = providers;
            _interval = interval;
            _repository = repository;
        }

        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            var logEntry = new HeartbeatLogEntry(Time.UtcNow);

            foreach (IHeartbeatProvider provider in _providers)
                logEntry.CollectFrom(provider, token);

            _repository.Insert(logEntry);

            return context.Wait(_interval);
        }
        
        public override string ToString()
        {
            return nameof(HeartbeatLoggingWorker);
        }
    }
}