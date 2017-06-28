using System;
using Castle.MicroKernel;

namespace Vertica.Integration.Domain.LiteServer
{
    public class RestartableContext : BackgroundServerContext
    {
        internal RestartableContext(IKernel kernel)
            : base(kernel)
        {
        }
        
        internal void OnException(AggregateException exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            LastException = exception;
            FailedCount++;
        }

        /// <summary>
        /// Gets the number of times this background server has failed.
        /// </summary>
        public uint FailedCount { get; private set; }

        /// <summary>
        /// Gets the Last Exception.
        /// </summary>
        public AggregateException LastException { get; private set; }
    }
}