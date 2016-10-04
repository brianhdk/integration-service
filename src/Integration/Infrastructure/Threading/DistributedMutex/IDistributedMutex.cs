using System;

namespace Vertica.Integration.Infrastructure.Threading.DistributedMutex
{
    public interface IDistributedMutex
    {
        IDisposable Enter(DistributedMutexContext context);
    }
}