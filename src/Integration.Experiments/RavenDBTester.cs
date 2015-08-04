using System;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Model;
using Vertica.Integration.RavenDB;
using Vertica.Integration.RavenDB.Infrastructure;

namespace Vertica.Integration.Experiments
{
	public static class RavenDbTester
	{
		public static ApplicationConfiguration TestRavenDb(this ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException("application");

			application
				.UseRavenDb(ravenDB => ravenDB
					.DefaultConnection(new DefaultRavenDb(ConnectionString.FromText("...")))
					.AddConnection(new AnotherRavenDb(ConnectionString.FromText("..."))))
				.Tasks(tasks => tasks.Task<RavenDBTesterTask>());

			return application;
		}

		public class RavenDBTesterTask : Task
		{
			private readonly IRavenDbFactory _ravenDB;

			public RavenDBTesterTask(IRavenDbFactory ravenDB)
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

	public class DefaultRavenDb : Connection
	{
		public DefaultRavenDb(ConnectionString connectionString)
			: base(connectionString)
		{
		}
	}

	public class AnotherRavenDb : Connection
	{
		public AnotherRavenDb(ConnectionString connectionString) 
			: base(connectionString)
		{
		}
	}
}