using System;
using System.Linq;
using MongoDB.Driver;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Model;
using Vertica.Integration.MongoDB;
using Vertica.Integration.MongoDB.Infrastructure;
using Vertica.Utilities_v4.Extensions.AnonymousExt;

namespace Vertica.Integration.Experiments
{
    public static class MongoDBExtensions
    {
        public static ApplicationConfiguration TestMongoDbTask(this ApplicationConfiguration application)
        {
            return application
                .Tasks(tasks => tasks
                    .Task<MongoDbTask>()
                    .MaintenanceTask(task => task
                        .Clear()
                        .IncludeLogRotator()))
                .UseMongoDB(mongoDB => mongoDB
                    .Connection(ConnectionString.FromText("mongodb://maersk-web01.vertica.dk/admin"))
                    .AddConnection(new AnalyticsDb()));
        }
    }

    public class MongoDbTask : Task
    {
        private readonly ITaskRunner _runner;
        private readonly ITaskFactory _factory;
        private readonly IMongoDBClientFactory _mongoDB;
        private readonly IMongoDBClientFactory<AnalyticsDb> _analyticsDB;

        public MongoDbTask(ITaskRunner runner, ITaskFactory factory, IMongoDBClientFactory<AnalyticsDb> analyticsDB, IMongoDBClientFactory mongoDB)
        {
            _runner = runner;
            _factory = factory;
            _analyticsDB = analyticsDB;
            _mongoDB = mongoDB;
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            if (_mongoDB.Database != null)
                context.Log.Message(String.Join(", ", Collections(_mongoDB.Database).Result));
            context.Log.Message(String.Join(", ", Collections(_analyticsDB.Database).Result));
            _runner.Execute(_factory.Get<MaintenanceTask>());
        }

        private async System.Threading.Tasks.Task<string[]> Collections(IMongoDatabase database)
        {
            var collections = await database.ListCollectionsAsync();

            var list = await collections.ToListAsync();

            return list.Select(x => x["name"].AsString).ToArray();
        }

        public override string Description
        {
            get { return "Test"; }
        }
    }

    public class AnalyticsDb : Connection
    {
        public AnalyticsDb()
            : base(ConnectionString.FromText("mongodb://maersk-web01.vertica.dk/analytics"))
        {
        }
    }
}