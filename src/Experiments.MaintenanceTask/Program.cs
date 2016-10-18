using System;
using System.Threading;
using Experiments.MaintenanceTask.Migrations.UCommerce;
using Vertica.Integration;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Model;
using Vertica.Integration.UCommerce;

namespace Experiments.MaintenanceTask
{
    class Program
    {
        static void Main(string[] args)
        {
            using (IApplicationContext context = ApplicationContext.Create(application => application
                .Database(database => database.IntegrationDb(ConnectionString.FromText("Server=.\\SQLExpress;Database=IS_MaintenanceTask;Trusted_Connection=True;")))
                .UseUCommerce(uCommerce => uCommerce.Database(ConnectionString.FromText("Server=.\\SQLExpress;Database=sc82-ucommerceSitecore_web;Trusted_Connection=True;")))
                .Migration(migration => migration.AddUCommerceFromNamespaceOfThis<M1_UCommerce>(DatabaseServer.SqlServer2014))
                .Tasks(tasks => tasks.MaintenanceTask(m => m.IncludeUCommerce()))))
            {
                var factory = context.Resolve<ITaskFactory>();
                var runner = context.Resolve<ITaskRunner>();
                var configuration = context.Resolve<IConfigurationService>();

                // migrate first
                runner.Execute(factory.Get("MigrateTask"));

                // do this in a migration
                var maintenanceConfiguration = configuration.Get<MaintenanceConfiguration>();

                //maintenanceConfiguration.ArchiveFolders.Clear();

                maintenanceConfiguration.ArchiveFolders.AddOrUpdate("UmbracoLogFiles", (folder, handler) =>
                {
                    folder.Path = @"c:\\tmp";
                    folder.ArchiveOptions.ExpiresAfter(TimeSpan.FromDays(365));

                    return handler.FilesOlderThan(TimeSpan.FromDays(-1), "b.txt");
                });

                configuration.Save(maintenanceConfiguration, "BHK");
                
                // run the maintenance task
                runner.Execute(factory.Get<Vertica.Integration.Domain.Core.MaintenanceTask>());
            }
        }
    }
}