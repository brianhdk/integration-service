using System;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Model;
using Vertica.Integration.RavenDB;
using Vertica.Integration.RavenDB.Infrastructure;

namespace Vertica.Integration.Experiments
{
	public static class RavenDBTester
	{
		public static ApplicationConfiguration TestRavenDB(this ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException("application");

			application
				.UseRavenDB(ravenDB => ravenDB
					.DefaultConnection(ConnectionString.FromText("..."))
					.AddConnection(new AnotherDb(ConnectionString.FromText("..."))))
				.Tasks(tasks => tasks.Task<RavenDBTesterTask>());

			return application;
		}

		public class RavenDBTesterTask : Task
		{
			private readonly IRavenDBFactory _ravenDB;

			public RavenDBTesterTask(IRavenDBFactory ravenDB)
			{
				_ravenDB = ravenDB;
			}

			public override string Description
			{
				get { return "Testing RavenDB"; }
			}

			public override void StartTask(ITaskExecutionContext context)
			{
				context.Log.Message(_ravenDB.DocumentStore.Url);
			}
		}

		public class Dto
		{
			public string Name { get; set; }
		}
	}

	public class AnotherDb : Connection
	{
		public AnotherDb(ConnectionString connectionString) : base(connectionString)
		{
		}
	}
}