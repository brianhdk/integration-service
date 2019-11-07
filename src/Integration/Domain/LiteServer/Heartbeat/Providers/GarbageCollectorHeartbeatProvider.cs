using System;
using System.Collections.Generic;
using System.Threading;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Domain.LiteServer.Heartbeat.Providers
{
    public class GarbageCollectorHeartbeatProvider : IHeartbeatProvider
    {
        public IEnumerable<string> CollectHeartbeatMessages(CancellationToken token)
        {
            yield return $"Total Memory: {GC.GetTotalMemory(forceFullCollection: true).ToPrettyFileSize()}";
        }
    }
}