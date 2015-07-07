using System;
using System.Linq;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4.Patterns;

namespace Vertica.Integration.Domain.Monitoring
{
    public class RedirectForMonitorTargets : IChainOfResponsibilityLink<MonitorEntry, Target[]>
    {
        private readonly Tuple<Target, MessageContainsText>[] _filters;

        public RedirectForMonitorTargets(params MonitorTarget[] targets)
        {
            _filters =
                (targets ?? new MonitorTarget[0])
                    .Select(target => Tuple.Create((Target)target, new MessageContainsText(target.ReceiveErrorsWithMessagesContaining)))
                    .ToArray();
        }

        public bool CanHandle(MonitorEntry context)
        {
            return _filters.Any(x => x.Item2.IsSatisfiedBy(context));
        }

        public Target[] DoHandle(MonitorEntry context)
        {
            return _filters.Where(x => x.Item2.IsSatisfiedBy(context)).Select(x => x.Item1).ToArray();
        }
    }
}