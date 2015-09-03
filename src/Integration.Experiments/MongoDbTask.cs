using System;
using MongoDB.Bson;
using MongoDB.Driver;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Model;
using Vertica.Integration.MongoDB;
using Vertica.Integration.MongoDB.Infrastructure;

namespace Vertica.Integration.Experiments
{
    public static class MongoDbExtensions
    {
        public static ApplicationConfiguration TestMongoDbTask(this ApplicationConfiguration application)
        {
            return application
                .Tasks(tasks => tasks
                    .Task<MongoDbTask>()
                    .MaintenanceTask(task => task
                        .Clear()
                        .IncludeLogRotator()))
                .UseMongoDb(mongoDB => mongoDB
					.DefaultConnection(new DefaultMongoDb(ConnectionString.FromText("mongodb://bhk:bhk2015!@ds036178.mongolab.com:36178/brianhdk")))
					.AddConnection(new AnotherMongoDb(ConnectionString.FromText("mongodb://contestant:veD8jepa@ds036178.mongolab.com:36178/brianhdk"))));
        }
    }

	public class DefaultMongoDb : Connection
	{
		public DefaultMongoDb(ConnectionString connectionString)
			: base(connectionString)
		{
		}
	}

	public class MongoDbTask : Task
    {
        private readonly ITaskRunner _runner;
        private readonly ITaskFactory _factory;
        private readonly IMongoDbClientFactory _defaultDb;
        private readonly IMongoDbClientFactory<AnotherMongoDb> _anotherDb;

        public MongoDbTask(ITaskRunner runner, ITaskFactory factory, IMongoDbClientFactory<AnotherMongoDb> anotherDb, IMongoDbClientFactory defaultDb)
        {
            _runner = runner;
            _factory = factory;
            _anotherDb = anotherDb;
            _defaultDb = defaultDb;
        }

        public override void StartTask(ITaskExecutionContext context)
        {
	        IMongoCollection<TestDto> dtos = _defaultDb.Database.GetCollection<TestDto>("testDtos");

			//foreach (var dto in dtos.Find(x => true).ToListAsync().Result)
			//{
			//	context.Log.Message(dto.Text);
			//}
			dtos.InsertOneAsync(new TestDto {Text = "Some text"}).Wait();

			//context.Log.Message("Inserted.");

	        //if (_mongoDB.Database != null)
	        //	context.Log.Message(String.Join(", ", Collections(_mongoDB.Database).Result));
	        //context.Log.Message(String.Join(", ", Collections(_analyticsDB.Database).Result));
	        //_runner.Execute(_factory.Get<MaintenanceTask>());
        }
		
        public override string Description
        {
            get { return "Test"; }
        }
    }

	public class TestDto
	{
		public ObjectId Id { get; set; }
		public string Text { get; set; }
	}

    public class AnotherMongoDb : Connection
    {
	    public AnotherMongoDb(ConnectionString connectionString)
			: base(connectionString)
	    {
	    }
    }
}