using System;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Model;
using Vertica.Integration.MongoDB.Infrastructure;
using Vertica.Integration.MongoDB.Maintenance;
using Vertica.Integration.MongoDB.Monitor;

namespace Vertica.Integration.MongoDB
{
    public static class MongoDbExtensions
    {
        public static ApplicationConfiguration UseMongoDb(this ApplicationConfiguration application, Action<MongoDbConfiguration> mongoDb)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));
            if (mongoDb == null) throw new ArgumentNullException(nameof(mongoDb));

			return application.Extensibility(extensibility =>
			{
				MongoDbConfiguration configuration = extensibility.Register(() => new MongoDbConfiguration(application));

				mongoDb(configuration);
			});
        }

        public static TaskConfiguration<MaintenanceWorkItem> IncludeLogRotator(this TaskConfiguration<MaintenanceWorkItem> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            return task.Step<LogRotatorStep>();
        }

        public static TaskConfiguration<MaintenanceWorkItem> IncludeLogRotator<TConnection>(this TaskConfiguration<MaintenanceWorkItem> task)
            where TConnection : Connection
        {
            return task.Step<LogRotatorStep<TConnection>>();
        }

        public static TaskConfiguration<MonitorWorkItem> IncludeMongoDb(this TaskConfiguration<MonitorWorkItem> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            return task.Step<PingMongoDbStep>();
        }

        public static TaskConfiguration<MonitorWorkItem> IncludeMongoDb<TConnection>(this TaskConfiguration<MonitorWorkItem> task)
            where TConnection : Connection
        {
            return task.Step<PingMongoDbStep<TConnection>>();
        }
    }
}