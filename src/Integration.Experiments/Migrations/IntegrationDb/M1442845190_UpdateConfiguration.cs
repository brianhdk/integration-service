using FluentMigrator;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Database.Migrations;

namespace Vertica.Integration.Experiments.Migrations.IntegrationDb
{
	[Migration(1442845190)]
    public class M1442845190_UpdateConfiguration : IntegrationMigration
    {
        public override void Up()
        {
            MonitorConfiguration configuration = GetConfiguration<MonitorConfiguration>();

	        SaveConfiguration(configuration);
        }

        public override void Down()
        {
        }
    }
}