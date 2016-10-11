using System;

namespace Vertica.Integration.Domain.LiteServer
{
    public sealed class BackgroundWorkerContinuation
    {
        internal BackgroundWorkerContinuation(TimeSpan? waitTime = null)
        {
            WaitTime = waitTime;
        }

        public TimeSpan? WaitTime { get; }
    }
}