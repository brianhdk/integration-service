using System;

namespace Vertica.Integration.Infrastructure.Threading.DistributedMutex
{
    public class DistributedMutexContext
    {
        private readonly Action<string> _onWaiting;

        public DistributedMutexContext(string name, DistributedMutexConfiguration configuration, Action<string> onWaiting = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty", nameof(name));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            Name = name;
            Configuration = configuration;
            _onWaiting = onWaiting;
        }

        public string Name { get; }

        public DistributedMutexConfiguration Configuration { get; }

        public void Waiting(string message)
        {
            _onWaiting?.Invoke(message);
        }
    }
}