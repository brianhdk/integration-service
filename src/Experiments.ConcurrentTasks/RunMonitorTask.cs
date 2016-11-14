using System;
using System.Threading;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Experiments.ConcurrentTasks
{
    public class RunMonitorTask : IBackgroundWorker
    {
        private readonly ITaskRunner _taskRunner;
        private readonly ITaskFactory _taskFactory;

        public RunMonitorTask(ITaskRunner taskRunner, ITaskFactory taskFactory, IConfigurationService configuration)
        {
            _taskRunner = taskRunner;
            _taskFactory = taskFactory;

            var monitorConfiguration = configuration.Get<MonitorConfiguration>();

            var service = monitorConfiguration.EnsureMonitorTarget(Target.Service);
            service.Recipients = new[] { "bhk@vertica.dk" };

            configuration.Save(monitorConfiguration, nameof(RunMonitorTask));
        }

        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            if (context.InvocationCount > 1)
                _taskRunner.Execute(_taskFactory.Get<MonitorTask>());

            return context.Wait(TimeSpan.FromMinutes(1));
        }
    }
}