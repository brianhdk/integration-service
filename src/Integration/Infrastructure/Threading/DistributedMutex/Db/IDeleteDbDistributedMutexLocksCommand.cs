using System;

namespace Vertica.Integration.Infrastructure.Threading.DistributedMutex.Db
{
    public interface IDeleteDbDistributedMutexLocksCommand
    {
        int Execute(DateTimeOffset olderThan);
    }
}