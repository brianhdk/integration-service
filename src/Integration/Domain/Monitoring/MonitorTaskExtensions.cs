using System;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Monitoring
{
    public static class MonitorTaskExtensions
    {
        public static TasksConfiguration MonitorTask(this TasksConfiguration configuration, Action<TaskConfiguration<MonitorWorkItem>> task = null)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            TaskConfiguration<MonitorWorkItem> localConfiguration = null;

            configuration = configuration
                .Task<MonitorTask, MonitorWorkItem>(x => localConfiguration = x);

            localConfiguration
                .Step<ExportIntegrationErrorsStep>()
                .Step<MonitorFoldersStep>()
                .Step<PingUrlsStep>();

            if (task != null)
                task(localConfiguration);

            return configuration;
        }        
    }
}