using System;
using FluentMigrator;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Model.Hosting.Handlers;

namespace Vertica.Integration.Experiments.Migrations.IntegrationDb
{
	[Migration(143827475)]
	public class M143827475_InstallTaskAsScheduleTask : IntegrationMigration
	{
		public override void Up()
		{
			InstallAsScheduleTask<DumpArchiveTask>(new Credentials("lbj@vertica.dk", "xxx"))
				.AddTaskAction<MaintenanceTask>()
				.AddTrigger(new OneTimeTrigger(startDate: DateTimeOffset.UtcNow.AddDays(2)))
				.AddTrigger(new DailyTrigger(startDate: DateTimeOffset.UtcNow.AddDays(2), recureDays: 20))
				.AddTrigger(new WeeklyTrigger(DateTimeOffset.UtcNow.AddDays(1), 1, TimeSpan.FromHours(3), DayOfWeek.Monday, DayOfWeek.Sunday))
				.InstallOrUpdate();

			UninstallScheduleTask<DumpArchiveTask>();
		}
	}
}