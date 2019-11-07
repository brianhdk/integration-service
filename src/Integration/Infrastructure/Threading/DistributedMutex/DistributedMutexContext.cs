using System;

namespace Vertica.Integration.Infrastructure.Threading.DistributedMutex
{
    public class DistributedMutexContext
    {
        private readonly Action<string> _onWaiting;

        public DistributedMutexContext(string name, DistributedMutexConfiguration configuration, Action<string> onWaiting = null, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty", nameof(name));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            Name = name;
            WaitTime = configuration.WaitTime;
            _onWaiting = onWaiting;
            Description = description;
        }

        /// <summary>
        /// Specifies the name of the DistributedMutex.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Specifies the maximum wait-time to wait for an existing lock to be released, before failing.
        /// </summary>
        public TimeSpan WaitTime { get; }

        /// <summary>
        /// Specifies an optional description about the lock to acquire.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Signals the client that we're waiting for an existing lock to be released.
        /// </summary>
        public void OnWaiting(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException(@"Value cannot be null or empty", nameof(message));
            
            _onWaiting?.Invoke(message);
        }
    }
}