using System.Collections.Generic;
using System.Threading;

namespace Vertica.Integration.Domain.LiteServer.Heartbeat
{
    public interface IHeartbeatProvider
    {
        IEnumerable<string> CollectHeartbeatMessages(CancellationToken token);
    }
}