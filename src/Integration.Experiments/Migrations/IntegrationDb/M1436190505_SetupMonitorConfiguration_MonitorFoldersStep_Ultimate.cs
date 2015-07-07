using System;
using FluentMigrator;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Database.Migrations;

namespace Vertica.Integration.Experiments.Migrations.IntegrationDb
{
    [Migration(1436190605)]
    public class M1436190605_SetupMonitorConfiguration_MonitorFoldersStep_Ultimate : IntegrationMigration
    {
        public override void Up()
        {
            using (var updater = UpdateConfiguration<MonitorConfiguration>())
            {
                var folders = updater.Configuration.MonitorFolders;

                folders.Clear();

                folders.Add((folder, criterias) =>
                {
                    folder.Path = @"D:\Dropbox\Development\NuGet.Packages";
                    return criterias.FilesOlderThan(TimeSpan.FromHours(1));
                });

                folders.Add((folder, criterias) =>
                {
                    folder.Path = @"D:\tmp\tst";
                    return criterias.FilesOlderThan(TimeSpan.FromSeconds(30));
                });

                folders.Enabled = false;
            }
        }

        public override void Down()
        {
        }
    }
}