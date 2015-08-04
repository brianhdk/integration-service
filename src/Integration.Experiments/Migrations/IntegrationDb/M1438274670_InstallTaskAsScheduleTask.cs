using System.Collections.Generic;
using System.Diagnostics;
using FluentMigrator;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting.Handlers;

namespace Vertica.Integration.Experiments.Migrations.IntegrationDb
{
	[Migration(143827469)]
	public class M143827469_InstallTaskAsScheduleTask : IntegrationMigration
	{
		public override void Up()
		{
			InstallAsScheduleTask<DumpArchiveTask>(new Credentials("lbj@vertica.dk", "xxx"), Arguments.Empty);

			UninstallScheduleTask<DumpArchiveTask>();
		}
	}
}