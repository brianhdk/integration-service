using FluentMigrator;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Database.Migrations;

namespace Vertica.Integration.Experiments.Migrations.IntegrationDb
{
    [Migration(1431659629)]
    public class M1431659629_Void : IntegrationMigration
    {
        public override void Up()
        {
            MonitorConfiguration configuration = GetConfiguration<MonitorConfiguration>();

            string s = "";
        }

        public override void Down()
        {
        }
    }
}