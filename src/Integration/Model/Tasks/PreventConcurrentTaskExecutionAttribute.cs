using System;
using Vertica.Integration.Infrastructure.Threading.DistributedMutex;

namespace Vertica.Integration.Model.Tasks
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PreventConcurrentTaskExecutionAttribute : Attribute
    {
        private uint? _waitTimeMs;

        internal DistributedMutexConfiguration Configuration
        {
            get
            {
                if (_waitTimeMs.HasValue)
                    return new DistributedMutexConfiguration(TimeSpan.FromMilliseconds(_waitTimeMs.Value));

                return null;
            }
        }

        /// <summary>
        /// Specifies the number of milliseconds to wait for a lock to become available.
        /// </summary>
        public uint WaitTimeMs
        {
            get { return _waitTimeMs.GetValueOrDefault(); }
            set { _waitTimeMs = value; }
        }

        /// <summary>
        /// Specifies a type that implements <see cref="IPreventConcurrentTaskExecutionRuntimeEvaluator" /> to be able to specify at runtime whether to prevent concurrent execution or not.
        /// </summary>
        public Type RuntimeEvaluator { get; set; }
    }
}