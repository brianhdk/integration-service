using System.Collections.Generic;
using Examples.LiteServer.Infrastructure.Hangfire;
using Examples.LiteServer.Tasks;
using FluentMigrator;
using Hangfire;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Model;

namespace Examples.LiteServer.Infrastructure.Migrations
{
    [Migration(1)]
    public class M1_SetupRecurringTasks : IntegrationMigration
    {
        public override void Up()
        {
            string importProductsTaskName = Task.NameOf<ImportProductsTask>();

            RecurringJob.AddOrUpdate<ITaskByNameRunner>(importProductsTaskName, 
                x => x.Run(importProductsTaskName, 
                    new KeyValuePair<string, string>("File", "C:\\tmp\\teammeeting\\products.csv"),
                    new KeyValuePair<string, string>("SkipDelete", string.Empty)), 
                Cron.Minutely);
        }
    }
}