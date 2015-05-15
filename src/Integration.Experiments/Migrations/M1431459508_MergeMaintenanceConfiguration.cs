using FluentMigrator;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure.Database.Migrations;

namespace Vertica.Integration.Experiments.Migrations
{
    [Migration(1431459508)]
    public class M1431459508_MergeMaintenanceConfiguration : IntegrationMigration
    {
        public override void Up()
        {
            MergeConfiguration<MaintenanceConfiguration>("Vertica.Integration.Domain.Core.MaintenanceConfiguration, Vertica.Integration");
        }

        public override void Down()
        {
        }
    }
}