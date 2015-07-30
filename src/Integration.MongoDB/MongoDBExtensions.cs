using System;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Model;

namespace Vertica.Integration.MongoDB
{
    public static class MongoDBExtensions
    {
        public static ApplicationConfiguration MongoDB(this ApplicationConfiguration application, Action<IMongoDBConfiguration> mongoDB)
        {
            if (application == null) throw new ArgumentNullException("application");
            if (mongoDB == null) throw new ArgumentNullException("mongoDB");

            var configuration = new MongoDBConfiguration(application);
            mongoDB(configuration);

            return application;
        }

        public static TaskConfiguration<MaintenanceWorkItem> IncludeLogRotator(this TaskConfiguration<MaintenanceWorkItem> task)
        {
            if (task == null) throw new ArgumentNullException("task");

            return task.Step<LogRotatorStep>();
        }
    }
}