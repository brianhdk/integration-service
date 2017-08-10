using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentMigrator;
using Hangfire;
using Vertica.Integration.Infrastructure.Database.Migrations;

namespace Experiments.Console.Hangfire.Migrations
{
    [Migration(1)]
    public class M1_SetupRecurringJobs : IntegrationMigration
    {
        public override void Up()
        {
            AddRecurringJob(nameof(MyTask), x => x.RunMyTask(), Cron.Minutely());

            // This calls the "RunTask()"-method on the IHangfireJob-interface
            AddRecurringJob(
                nameof(MyTaskWithProgressBar), 
                x => x.RunTask(
                    nameof(MyTaskWithProgressBar), 
                    new KeyValuePair<string, string>("Iterations", "10")), 
                Cron.Minutely());
        }

        public override void Down()
        {
            RecurringJob.RemoveIfExists(nameof(MyTask));
            RecurringJob.RemoveIfExists(nameof(MyTaskWithProgressBar));
        }

        private static void AddRecurringJob(string name, Expression<Action<IHangfireJob>> job, string cronExpression)
        {
            RecurringJob.AddOrUpdate(name, job, cronExpression);
        }
    }
}