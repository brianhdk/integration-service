using System;
using System.Net.Mail;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Logging.Elmah;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments.Monitoring
{
    public static class MonitorTaskTester
    {
        public static ApplicationConfiguration TestMonitorTask(this ApplicationConfiguration application)
        {
            return application
                .Tasks(tasks => tasks
                    .Task<ResetMonitorTask>()
                    .MonitorTask(task => task
                        .IncludeElmah()));
        }
    }

    public class ResetMonitorTask : Task
    {
        private readonly ITaskRunner _runner;
        private readonly ITaskFactory _factory;
        private readonly IConfigurationService _configuration;

        public ResetMonitorTask(ITaskRunner runner, ITaskFactory factory, IConfigurationService configuration)
        {
            _runner = runner;
            _factory = factory;
            _configuration = configuration;
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            var configuration = _configuration.Get<MonitorConfiguration>();
            configuration.LastRun = new DateTimeOffset(2015, 06, 01, 01, 01, 01, TimeSpan.Zero);

            MonitorTarget custom = configuration.EnsureMonitorTarget(Target.Custom("Custom"));
            custom.ReceiveErrorsWithMessagesContaining = new []
            {
                "A potentially dangerous Request.Path",
                "Operation is not valid due to the current state of the object"
            };
            
            custom.MailPriority = MailPriority.High;

            foreach (MonitorTarget target in configuration.Targets)
                target.Recipients = new[] { "bhk@vertica.dk" };

            _configuration.Save(configuration, "ResetMonitorTask");

            _runner.Execute(_factory.Get<MonitorTask>());
        }

        public override string Description => "test";
    }
}