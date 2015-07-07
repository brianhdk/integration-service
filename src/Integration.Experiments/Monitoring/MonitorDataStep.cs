using System;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments.Monitoring
{
    public class MonitorDataStep : Step<MonitorWorkItem>
    {
        public override string Description
        {
            get { return "Monitors data end-points"; }
        }

        public override void Execute(MonitorWorkItem workItem, ITaskExecutionContext context)
        {
            throw new NotImplementedException();

            // check for data problemer - fx pj eksemplet
            // check for manglende behandlede ordrer i uCommerce (tegn på at integrationer ikke kører)
        }
    }
}