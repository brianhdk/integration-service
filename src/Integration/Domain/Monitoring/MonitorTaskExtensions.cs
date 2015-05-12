using System;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Monitoring
{
    public static class MonitorTaskExtensions
    {
        public static TasksConfiguration AddMonitorTask(this TasksConfiguration configuration, Action<TaskConfiguration<MonitorWorkItem>> task = null)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            TaskConfiguration<MonitorWorkItem> taskConfiguration = null;

            configuration = configuration
                .Add<MonitorTask, MonitorWorkItem>(x => taskConfiguration = x);

            taskConfiguration
                .Step<ExportIntegrationErrorsStep>()
                .Step<PingUrlsStep>();

            if (task != null)
                task(taskConfiguration);

            return configuration;
        }        
    }
}