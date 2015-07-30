using System;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Model;

namespace Vertica.Integration.MongoDB
{
    public static class MongoDBExtensions
    {
        public static ApplicationConfiguration UseMongoDB(this ApplicationConfiguration application, Action<IMongoDBConfiguration> mongoDB)
        {
            if (application == null) throw new ArgumentNullException("application");
            if (mongoDB == null) throw new ArgumentNullException("mongoDB");

			return application.Extensibility(extensibility =>
			{
				MongoDBConfiguration configuration = extensibility.Cache(() => new MongoDBConfiguration(application));

				mongoDB(configuration);
			});
        }

        public static TaskConfiguration<MaintenanceWorkItem> IncludeLogRotator(this TaskConfiguration<MaintenanceWorkItem> task)
        {
            if (task == null) throw new ArgumentNullException("task");

            return task.Step<LogRotatorStep>();
        }
    }
}