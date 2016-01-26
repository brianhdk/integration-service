using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Castle.MicroKernel;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Runner.Processors.SQLite;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Utilities_v4.Extensions.AttributeExt;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    public class MigrateTask : Task
    {
        private readonly IKernel _kernel;
        private readonly MigrationDb[] _dbs;
        private readonly IDisposable _loggingDisabler;
        private readonly bool _databaseCreated;
	    private readonly ITaskFactory _taskFactory;
	    private readonly ITaskRunner _taskRunner;

        public MigrateTask(Func<IDbFactory> db, ILogger logger, IKernel kernel, IMigrationDbs dbs, ITaskFactory taskFactory, ITaskRunner taskRunner)
        {
            _kernel = kernel;
	        _taskFactory = taskFactory;
	        _taskRunner = taskRunner;

	        if (!dbs.IntegrationDbDisabled)
	        {
		        string connectionString = EnsureIntegrationDb(db(), dbs.CheckExistsAndCreateIntegrationDbIfNotFound, out _databaseCreated);

		        var integrationDb = new IntegrationMigrationDb(
			        dbs.IntegrationDbDatabaseServer,
			        ConnectionString.FromText(connectionString),
			        typeof (M1_Baseline).Assembly,
			        typeof (M1_Baseline).Namespace);

		        StringBuilder output;
		        MigrationRunner runner = CreateRunner(integrationDb, out output);

		        // Latest migration has not been applied, so we'll have to disable any logging.
		        if (!runner.VersionLoader.VersionInfo.HasAppliedMigration(FindLatestMigration()))
			        _loggingDisabler = logger.Disable();

		        dbs = dbs.WithIntegrationDb(integrationDb);
	        }

	        _dbs = dbs.ToArray();
        }

        private static long FindLatestMigration()
        {
            long latestMigration =
                typeof (M1_Baseline).Assembly.GetTypes()
                    .Where(x =>
                        x.IsClass &&
                        x.Namespace == typeof (M1_Baseline).Namespace)
                    .Select(x =>
                    {
                        var migration = x.GetAttribute<MigrationAttribute>();

                        return migration != null ? migration.Version : -1;
                    })
                    .OrderByDescending(x => x)
                    .First();

            return latestMigration;
        }

        public override string Description
        {
            get { return "Runs migrations against all configured databases. Will also execute any custom task if provided by Arguments."; }
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            bool enableLogger = _loggingDisabler != null;

	        MigrationDb[] destinations = _dbs;

			string[] names = (context.Arguments["Names"] ?? String.Empty)
				.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);

	        if (names.Length > 0)
	        {
		        ILookup<string, MigrationDb> migrationsByName = 
					_dbs.ToLookup(db => db.IdentifyingName, db => db, StringComparer.OrdinalIgnoreCase);

				destinations = names
					.SelectMany(x => migrationsByName[x])
					.ToArray();
	        }

	        string action = (context.Arguments["Action"] ?? String.Empty).ToLowerInvariant();

			foreach (MigrationDb destination in destinations)
            {
                StringBuilder output;
                MigrationRunner runner = CreateRunner(destination, out output);

	            switch (action)
	            {
					case "list":
			            destination.List(runner, context, _kernel);
			            break;
					case "rollback":
			            destination.Rollback(runner, context, _kernel);
			            break;
		            default:
			            destination.MigrateUp(runner, context, _kernel);
			            break;
	            }
                
				runner.Processor.Dispose();
				
                if (output.Length > 0)
                    context.Log.Message(output.ToString());

                if (enableLogger)
                {
                    _loggingDisabler.Dispose();
                    enableLogger = false;
                }
            }
        }

        public override void End(EmptyWorkItem workItem, ITaskExecutionContext context)
        {
            if (_databaseCreated)
            {
                context.Log.Warning(
                    Target.Service,
                    "Created new database (using Simple Recovery) and applied migrations to this. Make sure to configure this new database (auto growth, backup etc).");
            }

	        string[] taskNames = (context.Arguments["Tasks"] ?? String.Empty)
				.Split(new[] {",", ";"}, StringSplitOptions.RemoveEmptyEntries);

	        foreach (string taskName in taskNames)
	        {
		        ITask task;
		        if (_taskFactory.TryGet(taskName, out task))
		        {
			        if (task is MigrateTask)
				        continue;

			        _taskRunner.Execute(task);
		        }
				else
		        {
			        context.Log.Warning(Target.Service, "Task with name '{0}' not found.", taskName);
		        }
	        }
        }

        private static string EnsureIntegrationDb(IDbFactory db, bool checkExistsAndCreateIntegrationDbIfNotFound, out bool databaseCreated)
        {
            using (IDbConnection connection = db.GetConnection())
            {
                if (!checkExistsAndCreateIntegrationDbIfNotFound)
                {
                    databaseCreated = false;
                    return connection.ConnectionString;
                }

                using (IDbCommand command = connection.CreateCommand())
                {
                    string databaseName = connection.Database;

                    var builder = new SqlConnectionStringBuilder(connection.ConnectionString);

                    Func<string, string> changeDatabase = dbName =>
                    {
                        builder["Initial Catalog"] = dbName;
                        return builder.ConnectionString;
                    };

                    connection.ConnectionString = changeDatabase("master");
                    connection.Open();

                    command.CommandText = @"
IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = @DbName)
	BEGIN

		EXECUTE ('
			CREATE DATABASE ' + @DbName + ';
			ALTER DATABASE ' + @DbName +' SET RECOVERY SIMPLE
		')
		
		SELECT 'CREATED'
	END
ELSE
	SELECT 'EXISTS'
";

                    IDbDataParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "DbName";
                    parameter.Value = databaseName;

                    command.Parameters.Add(parameter);

                    databaseCreated = ((string)command.ExecuteScalar() == "CREATED");

                    return changeDatabase(databaseName);
                }                
            }
        }

        private MigrationRunner CreateRunner(MigrationDb db, out StringBuilder output)
        {
            var sb = output = new StringBuilder();

            var announcer = new TextWriterAnnouncer(s =>
            {
                if (sb.Length == 0)
                    sb.AppendLine();

                sb.Append(s);
            });

            IMigrationProcessorFactory factory = CreateFactory(db.DatabaseServer);
            IMigrationProcessor processor = factory.Create(db.ConnectionString, announcer, new MigrationOptions());

            var context = new RunnerContext(announcer)
            {
                Namespace = db.NamespaceContainingMigrations,
                ApplicationContext = _kernel
            };

            return new MigrationRunner(db.Assembly, context, processor);
        }

        private static IMigrationProcessorFactory CreateFactory(DatabaseServer databaseServer)
        {
            switch (databaseServer)
            {
                case DatabaseServer.SqlServer2012:
                    return new SqlServer2012ProcessorFactory();
                case DatabaseServer.SqlServer2014:
                    return new SqlServer2014ProcessorFactory();
				case DatabaseServer.Sqlite:
					return new SQLiteProcessorFactory();
                default:
                    throw new ArgumentOutOfRangeException("databaseServer");
            }
        }

        private class MigrationOptions : IMigrationProcessorOptions
        {
            public MigrationOptions()
            {
                PreviewOnly = false;
                ProviderSwitches = null;
                Timeout = 60;
            }

            public bool PreviewOnly { get; private set; }
            public string ProviderSwitches { get; private set; }
            public int Timeout { get; private set; }
        }
    }
}