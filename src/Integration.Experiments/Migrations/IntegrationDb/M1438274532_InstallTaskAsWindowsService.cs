using System;
using FluentMigrator;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Model.Hosting.Handlers;
using Vertica.Integration.WebApi;

namespace Vertica.Integration.Experiments.Migrations.IntegrationDb
{
	[Migration(1438274543)]
	public class M1438274532_InstallTaskAsWindowsService : IntegrationMigration
    {
        public override void Up()
        {
			//InstallAsWindowsService<DumpArchiveTask>(install => install
			//	.Repeat(TimeSpan.FromSeconds(30)));

			//UninstallWindowsService<DumpArchiveTask>();

			//this.InstallWebApiHostAsWindowsService("http://localhost:8123");
			//this.UninstallWebApiHostWindowsService();
        }
    }
}