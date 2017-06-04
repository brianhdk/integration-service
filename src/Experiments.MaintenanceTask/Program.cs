using System;
using Experiments.MaintenanceTask.Migrations.UCommerce;
using Vertica.Integration;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Model;
using Vertica.Integration.MongoDB;
using Vertica.Integration.MongoDB.Commands;
using Vertica.Integration.UCommerce;

namespace Experiments.MaintenanceTask
{
    class Program
    {
        static void Main(string[] args)
        {
            using (IApplicationContext context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                //        .Connection(ConnectionString.FromText("Server=.\\SQLExpress;Database=IS_MaintenanceTask;Trusted_Connection=True;"))
                        .Disable()
                ))
                .UseMongoDb(mongoDB => mongoDB
                    // http://docs.mongodb.org/manual/reference/connection-string/#connections-connection-options
                    .DefaultConnection(ConnectionString.FromText("mongodb://localhost"))
                    //.AddConnection(new CustomMongoDb(ConnectionString.FromText("mongodb://localhost")))
                )
                .UseUCommerce(uCommerce => uCommerce
                    .Connection(ConnectionString.FromText("Integrated Security=SSPI;Data Source=hoka-sql01.vertica.dk;Database=Hoka_Sitecore_uCommerce")))
                //.Migration(migration => migration.AddUCommerceFromNamespaceOfThis<M1_UCommerce>(DatabaseServer.SqlServer2014))
                .Tasks(tasks => tasks
                    .MaintenanceTask(m => m
                        //.IncludeLogRotator()
                        //.IncludeLogRotator<CustomMongoDb>()
                        .IncludeUCommerce()
                    )
                )))
            {
                var factory = context.Resolve<ITaskFactory>();
                var runner = context.Resolve<ITaskRunner>();

                //var mongoDb = context.Resolve<IMongoDbClientFactory>();
                //var rotator = context.Resolve<ILogRotatorCommand>();

                //rotator.Execute(mongoDb.Client);


                //var configuration = context.Resolve<IConfigurationService>();

                // migrate first
                //runner.Execute(factory.Get("MigrateTask"));

                // do this in a migration
                //var maintenanceConfiguration = configuration.Get<MaintenanceConfiguration>();

                //maintenanceConfiguration.ArchiveFolders.Clear();

                //maintenanceConfiguration.ArchiveFolders.AddOrUpdate("UmbracoLogFiles", (folder, handler) =>
                //{
                //    folder.Path = @"c:\\tmp";
                //    folder.ArchiveOptions.ExpiresAfter(TimeSpan.FromDays(365));

                //    return handler.FilesOlderThan(TimeSpan.FromDays(-1), "b.txt");
                //});

                //configuration.Save(maintenanceConfiguration, "BHK");

                // run the maintenance task
                runner.Execute(factory.Get<Vertica.Integration.Domain.Core.MaintenanceTask>());
            }
        }

        private class CustomMongoDb : Vertica.Integration.MongoDB.Infrastructure.Connection
        {
            public CustomMongoDb(ConnectionString connectionString) : base(connectionString)
            {
            }
        }
    }
}