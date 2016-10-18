using FluentMigrator;
using Vertica.Integration.Infrastructure.Database.Migrations;

namespace Experiments.MaintenanceTask.Migrations.UCommerce
{
    [Migration(1)]
    public class M1_UCommerce : IntegrationMigration
    {
        public override void Up()
        {
            Create.Table("CustomTable")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString(50);
        }
    }
}