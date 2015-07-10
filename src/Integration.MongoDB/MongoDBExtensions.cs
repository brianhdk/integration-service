using System;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Model;

namespace Vertica.Integration.MongoDB
{
    public static class MongoDBExtensions
    {
        public static ApplicationConfiguration MongoDB(this ApplicationConfiguration builder, Action<IMongoDBConfiguration> mongoDB)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (mongoDB == null) throw new ArgumentNullException("mongoDB");

            var configuration = new MongoDBConfiguration();
            mongoDB(configuration);

            builder.AddCustomInstallers(configuration.CustomInstallers);

            return builder;
        }

        public static TaskConfiguration<MaintenanceWorkItem> IncludeLogRotator(this TaskConfiguration<MaintenanceWorkItem> task)
        {
            if (task == null) throw new ArgumentNullException("task");

            return task.Step<LogRotatorStep>();
        }
    }
}