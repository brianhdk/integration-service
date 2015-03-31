using FluentMigrator;

namespace Vertica.Integration.Experiments.Extend_IntegrationDb
{
    [Migration(1427839040)]
    public class M1427839040_DeleteNewTable : Migration
    {
        public override void Up()
        {
            Delete.Table("NewTable");
        }

        public override void Down()
        {
        }
    }
}