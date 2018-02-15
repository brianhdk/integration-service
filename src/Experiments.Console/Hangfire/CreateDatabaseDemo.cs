using System;
using System.Data;
using System.Data.SqlClient;
using Hangfire;
using Hangfire.SqlServer;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Hangfire;
using Vertica.Integration.Infrastructure;

namespace Experiments.Console.Hangfire
{
    public static class CreateDatabaseDemo
    {
        public static void Run()
        {
            var db = ConnectionString.FromText(@"Integrated Security=SSPI;Data Source=.\SQLExpress;Database=IntegrationService_Hangfire");

            RemoveDatabaseIfExists(db);

            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .PrefixTables("IntegrationService.")
                        .Connection(db)))
                .UseHangfire(hangfire => hangfire
                    .AddToLiteServer()
                    .Configuration(configuration =>
                    {
                        configuration.UseSqlServerStorage(db, new SqlServerStorageOptions
                        {
                            QueuePollInterval = TimeSpan.FromSeconds(5)
                        });
                    }))
                .UseLiteServer(liteServer => liteServer
                    .OnStartup(startup => startup.RunMigrateTask()))))
            {
                context.Execute(nameof(LiteServerHost));
            }
        }

        private static void RemoveDatabaseIfExists(ConnectionString connectionString)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                using (IDbCommand command = connection.CreateCommand())
                {
                    string databaseName = connection.Database;

                    var builder = new SqlConnectionStringBuilder(connection.ConnectionString);

                    string ChangeDatabase(string dbName)
                    {
                        builder["Initial Catalog"] = dbName;
                        return builder.ConnectionString;
                    }

                    connection.ConnectionString = ChangeDatabase("master");
                    connection.Open();

                    command.CommandText = @"
IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = @DbName)
BEGIN
	EXECUTE ('DROP DATABASE ' + @DbName + ';')
END
";

                    IDbDataParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "DbName";
                    parameter.Value = databaseName;

                    command.Parameters.Add(parameter);

                    command.ExecuteScalar();
                }
            }
        }
    }
}