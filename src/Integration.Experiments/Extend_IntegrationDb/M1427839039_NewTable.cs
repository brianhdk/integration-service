using FluentMigrator;

namespace Vertica.Integration.Experiments.Extend_IntegrationDb
{
    [Migration(1427839039)]
    public class M1427839039_NewTable : Migration
    {
        public override void Up()
        {
            Create.Table("NewTable")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity();
        }

        public override void Down()
        {
        }
    }
}