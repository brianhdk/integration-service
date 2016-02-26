using System;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Experiments.Migrations.IntegrationDb;

namespace Vertica.Integration.Experiments
{
    public static class ConfigurationExtensions
    {
        public static ApplicationConfiguration NoDb(this ApplicationConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

	        return configuration
		        .Database(database => database.DisableIntegrationDb());
        }

        public static ApplicationConfiguration RegisterMigrations(this ApplicationConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            return configuration
                .Migration(migration => migration.AddFromNamespaceOfThis<M1427839041_SetupMonitorConfiguration>());
        }

        public static ApplicationConfiguration RegisterTasks(this ApplicationConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            return configuration.Tasks(tasks => tasks
                .AddFromAssemblyOfThis<HelloTask>()
                .MonitorTask()
                .MaintenanceTask());
        }

        public static ApplicationConfiguration Void(this ApplicationConfiguration configuration)
        {
            return configuration;
        }
    }
}