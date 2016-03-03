using FluentMigrator;
using Hangfire;
using Hangfire.Example.Shared;
using Vertica.Integration.Model;

namespace Integration.Experiments.Hangfire.Migrations
{
	[Migration(1456825440)]
	public class M1456825440_HangfireScheduledTask : Migration
	{
		public override void Up()
		{
			RecurringJob.AddOrUpdate<ITaskByNameRunner>(Task.NameOf<ExportCatalogTask>(), x => x.Run(Task.NameOf<ExportCatalogTask>()), Cron.Minutely);
		}

		public override void Down()
		{
		}
	}
}