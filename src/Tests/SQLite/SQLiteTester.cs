using System;
using System.IO;
using FluentMigrator;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Model;
using Vertica.Integration.SQLite;

namespace Vertica.Integration.Tests.SQLite
{
	[TestFixture(Category = "Integration,Slow")]
	public class SQLiteTester
	{
		private string _fileName;
		private IApplicationContext _context;

		[SetUp]
		public void Initialize()
		{
			_fileName = String.Format("Data\\{0:N}.sqlite", Guid.NewGuid());

			var connection = new TestSqliteConnection(_fileName);

			_context = TestableApplicationContext.Create(application => application
				.Database(database => database.SQLite(sqlite => sqlite.AddConnection(connection)))
				.Migration(migration => migration.AddFromNamespaceOfThis<M1_CreateMyTable>(DatabaseServer.Sqlite, connection.ConnectionString))
				.Tasks(tasks => tasks.Clear().Task<MigrateTask>().AddFromAssemblyOfThis<SQLiteTester>()));
		}

		[TearDown]
		public void Dispose()
		{
			_context.Dispose();

			File.Delete(_fileName);
		}

		[TestCase("TestMigrationsTask")]
		public void ExecuteTask(string taskName)
		{
			_context.Execute(taskName);
		}

		public class TestMigrationsTask : Task
		{
			private readonly ITaskFactory _taskFactory;
			private readonly ITaskRunner _taskRunner;
			private readonly IDbFactory<TestSqliteConnection> _db;

			public TestMigrationsTask(ITaskFactory taskFactory, ITaskRunner taskRunner, IDbFactory<TestSqliteConnection> db)
			{
				_taskFactory = taskFactory;
				_taskRunner = taskRunner;
				_db = db;
			}

			public override void StartTask(ITaskExecutionContext context)
			{
				_taskRunner.Execute(_taskFactory.Get<MigrateTask>());

				using (var session = _db.OpenSession())
				{
					int id = session.ExecuteScalar<int>(@"
INSERT INTO MyTable (Name) VALUES (@Name);
SELECT last_insert_rowid();", new { Name = "Test" });

					context.Log.Message("Number of rows: {0}", id);
				}
			}

			public override string Description
			{
				get { return "Some description"; }
			}
		}

		public class TestSqliteConnection : SQLiteConnection
		{
			public TestSqliteConnection(string fileName)
				: base(FromCurrentDirectory(fileName))
			{
			}
		}

		[Migration(1)]
		public class M1_CreateMyTable : Migration
		{
			public override void Up()
			{
				Create.Table("MyTable")
					.WithColumn("ID").AsInt32().PrimaryKey().Identity()
					.WithColumn("Name").AsString().NotNullable();
			}

			public override void Down()
			{
				throw new NotSupportedException();
			}
		}
	}
}