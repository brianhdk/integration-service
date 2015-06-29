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
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Utilities_v4.Extensions.AttributeExt;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    public class MigrateTask : Task
    {
        private readonly IKernel _kernel;
        private readonly MigrationTarget[] _targets;
        private readonly IDisposable _loggingDisabler;
        private readonly bool _databaseCreated;

        public MigrateTask(Lazy<IDbFactory> db, ILogger logger, IKernel kernel, MigrationConfiguration configuration)
        {
            _kernel = kernel;
            _targets = configuration.CustomTargets;

            if (!configuration.IntegrationDbDisabled)
            {
                string connectionString = EnsureIntegrationDb(db.Value, configuration.CheckExistsIntegrationDb, out _databaseCreated);

                var integrationDb = new MigrationTarget(
                    configuration.IntegrationDbDatabaseServer,
                    ConnectionString.FromText(connectionString),
                    typeof(M1_Baseline).Assembly,
                    typeof(M1_Baseline).Namespace);

                StringBuilder output;
                MigrationRunner runner = CreateRunner(integrationDb, out output);

                // Latest migration has not been applied, so we'll have to disable any logging.
                if (!runner.VersionLoader.VersionInfo.HasAppliedMigration(FindLatestMigration()))
                    _loggingDisabler = logger.Disable();

                _targets = new[] { integrationDb }.Concat(_targets).ToArray();                
            }
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
            get { return "Runs migrations."; }
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            bool enableLogger = _loggingDisabler != null;

            foreach (MigrationTarget destination in _targets)
            {
                StringBuilder output;
                MigrationRunner runner = CreateRunner(destination, out output);

                runner.MigrateUp(useAutomaticTransactionManagement: true);

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
        }

        private static string EnsureIntegrationDb(IDbFactory db, bool checkExistsIntegrationDb, out bool databaseCreated)
        {
            using (IDbConnection connection = db.GetConnection())
            {
                if (!checkExistsIntegrationDb)
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

        private MigrationRunner CreateRunner(MigrationTarget target, out StringBuilder output)
        {
            var sb = output = new StringBuilder();

            var announcer = new TextWriterAnnouncer(s =>
            {
                if (sb.Length == 0)
                    sb.AppendLine();

                sb.Append(s);
            });

            IMigrationProcessorFactory factory = CreateFactory(target.DatabaseServer);
            IMigrationProcessor processor = factory.Create(target.ConnectionString, announcer, new MigrationOptions());

            var context = new RunnerContext(announcer)
            {
                Namespace = target.NamespaceContainingMigrations,
                ApplicationContext = _kernel
            };

            return new MigrationRunner(target.Assembly, context, processor);
        }

        private static IMigrationProcessorFactory CreateFactory(DatabaseServer databaseServer)
        {
            switch (databaseServer)
            {
                case DatabaseServer.SqlServer2012:
                    return new SqlServer2012ProcessorFactory();
                case DatabaseServer.SqlServer2014:
                    return new SqlServer2014ProcessorFactory();
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