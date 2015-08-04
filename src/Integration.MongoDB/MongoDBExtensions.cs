using System;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Model;
using Vertica.Integration.MongoDB.Maintenance;

namespace Vertica.Integration.MongoDB
{
    public static class MongoDbExtensions
    {
        public static ApplicationConfiguration UseMongoDb(this ApplicationConfiguration application, Action<MongoDbConfiguration> mongoDb)
        {
            if (application == null) throw new ArgumentNullException("application");
            if (mongoDb == null) throw new ArgumentNullException("mongoDb");

			return application.Extensibility(extensibility =>
			{
				MongoDbConfiguration configuration = extensibility.Cache(() => new MongoDbConfiguration(application));

				mongoDb(configuration);
			});
        }

        public static TaskConfiguration<MaintenanceWorkItem> IncludeLogRotator(this TaskConfiguration<MaintenanceWorkItem> task)
        {
            if (task == null) throw new ArgumentNullException("task");

            return task.Step<LogRotatorStep>();
        }
    }
}