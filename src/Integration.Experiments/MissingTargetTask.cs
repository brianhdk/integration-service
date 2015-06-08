using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class MissingTargetTask : Task
    {
        private readonly ITaskFactory _factory;
        private readonly ITaskRunner _runner;

        public MissingTargetTask(ITaskFactory factory, ITaskRunner runner)
        {
            _factory = factory;
            _runner = runner;
        }

        public override string Description
        {
            get { return "TBD"; }
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            context.Log.Warning(Target.Custom("TestTarget"), "Test");
            _runner.Execute(_factory.Get<MonitorTask>());
        }
    }
}