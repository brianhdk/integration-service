using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using Vertica.Integration.Infrastructure.Database.Dapper;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    public class MigrateTask : Task
    {
        private readonly MigrationDestination[] _destinations;
        private readonly IDisposable _loggingDisabler;
        private readonly bool _databaseCreated;

        public MigrateTask(IDapperProvider dapper, ILogger logger, MigrationConfiguration configuration)
        {
            string connectionString = EnsureIntegrationDb(dapper, out _databaseCreated);

            var selfDestination = new MigrationDestination(
                configuration.IntegrationDbDatabaseServer,
                connectionString,
                typeof (M1_Baseline).Assembly,
                typeof (M1_Baseline).Namespace);

            StringBuilder output;
            MigrationRunner runner = CreateRunner(selfDestination, out output);

            // Baseline has not been applied, so we'll have to disable any logging
            if (!runner.VersionLoader.VersionInfo.HasAppliedMigration(M1_Baseline.VersionNumber))
                _loggingDisabler = logger.Disable();

            _destinations = new[] { selfDestination }.Concat(configuration.CustomDestinations).ToArray();
        }

        public override string Description
        {
            get { return "Runs migrations."; }
        }

        public override void StartTask(Log log, params string[] arguments)
        {
            foreach (MigrationDestination destination in _destinations)
            {
                StringBuilder output;
                MigrationRunner runner = CreateRunner(destination, out output);

                runner.MigrateUp(useAutomaticTransactionManagement: true);

                if (output.Length > 0)
                    log.Message(output.ToString());
            }
        }

        public override void End(EmptyWorkItem workItem, Log log, params string[] arguments)
        {
            if (_loggingDisabler != null)
                _loggingDisabler.Dispose();

            if (_databaseCreated)
            {
                log.Warning(
                    Target.Service,
                    "Created new database (using Simple Recovery) and applied migrations to this. Make sure to configure this new database (auto growth, backup etc).");
            }
        }

        private static string EnsureIntegrationDb(IDapperProvider dapper, out bool databaseCreated)
        {
            using (IDbConnection connection = dapper.GetConnection())
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

                databaseCreated = ((string) command.ExecuteScalar() == "CREATED");

                return changeDatabase(databaseName);
            }
        }

        private static MigrationRunner CreateRunner(MigrationDestination destination, out StringBuilder output)
        {
            var sb = output = new StringBuilder();

            var announcer = new TextWriterAnnouncer(s =>
            {
                if (sb.Length == 0)
                    sb.AppendLine();

                sb.Append(s);
            });

            IMigrationProcessorFactory factory = CreateFactory(destination.DatabaseServer);

            IMigrationProcessor processor = factory.Create(destination.ConnectionString, announcer, new MigrationOptions());

            var context = new RunnerContext(announcer)
            {
                Namespace = destination.NamespaceContainingMigrations
            };

            return new MigrationRunner(destination.Assembly, context, processor);
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
