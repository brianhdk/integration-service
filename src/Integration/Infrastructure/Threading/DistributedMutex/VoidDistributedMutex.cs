using System;

namespace Vertica.Integration.Infrastructure.Threading.DistributedMutex
{
    public sealed class VoidDistributedMutex : IDistributedMutex
    {
        public IDisposable Enter(DistributedMutexContext context)
        {
            return null;
        }
    }
}