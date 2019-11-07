using System;
using System.Diagnostics;
using System.Threading;

namespace Vertica.Integration.Tests.Infrastructure.Testing
{
    public class WaitBlock : IDisposable
    {
        private readonly TimeSpan _waitTime;

        public WaitBlock(TimeSpan? waitTime = null)
        {
            _waitTime = waitTime.GetValueOrDefault(Debugger.IsAttached
                ? TimeSpan.FromMinutes(5)
                : TimeSpan.FromSeconds(5));

            ResetEvent = new ManualResetEvent(false);
        }

        private ManualResetEvent ResetEvent { get; }

        public void Wait()
        {
            ResetEvent.WaitOne(_waitTime);
        }

        public void Release()
        {
            ResetEvent.Set();
        }

        public void Dispose()
        {
            ResetEvent.Dispose();
        }
    }
}