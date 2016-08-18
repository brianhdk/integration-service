using System;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Monitoring
{
    public static class MonitorTaskExtensions
    {
        public static TasksConfiguration MonitorTask(this TasksConfiguration configuration, Action<TaskConfiguration<MonitorWorkItem>> task = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            TaskConfiguration<MonitorWorkItem> localConfiguration = null;

            configuration = configuration
                .Task<MonitorTask, MonitorWorkItem>(x => localConfiguration = x);

            localConfiguration
                .Step<MonitorFoldersStep>()
                .Step<PingUrlsStep>()
				.Step<ExportIntegrationErrorsStep>();

	        task?.Invoke(localConfiguration);

	        return configuration;
        }        
    }
}