using System;

namespace Vertica.Integration.Infrastructure.Threading.DistributedMutex
{
    internal sealed class ThrowingDistributedMutex : IDistributedMutex
    {
        public IDisposable Enter(DistributedMutexContext context)
        {
            throw new NotSupportedException($"{nameof(ThrowingDistributedMutex)}");
        }
    }
}