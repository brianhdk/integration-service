using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Vertica.Integration.Model;
using Vertica.Integration.SQLite;

namespace Vertica.Integration.Experiments.SQLite
{
	public static class SQLiteTester
	{
		public static ApplicationConfiguration TestSQLite(this ApplicationConfiguration application)
		{
			return application
				.Database(database => database.SQLite())
				.Tasks(tasks => tasks.Task<SqlLiteTask>());
		}
	}

	public class SqlLiteTask : Task
	{
		public override string Description
		{
			get { return "Test"; }
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			string file = Path.Combine(Environment.CurrentDirectory, "Db.sqlite");

			bool createDb = !File.Exists(file);

			context.Log.Message(Environment.CurrentDirectory);
			using (IDbConnection conn = new SQLiteConnection(String.Format("Data Source={0}", file)))
			using (IDbCommand command = conn.CreateCommand())
			{
				conn.Open();

				if (createDb)
				{
					command.CommandText = @"create table Customer
(
    ID                                  integer primary key AUTOINCREMENT,
    FirstName                           varchar(100) not null
)";

					command.ExecuteNonQuery();
				}

				command.CommandText = @"INSERT INTO Customer 
            ( FirstName ) VALUES 
            ( 'FIRST' );
            select last_insert_rowid()";

				context.Log.Message(command.ExecuteScalar().ToString());
			}
			
		}
	}
}