using FluentMigrator;
using Vertica.Integration.Infrastructure.Database.Migrations;

namespace Vertica.Integration.Experiments.Migrations.CustomDb
{
    [Migration(1431659519)]
    public class M1431659519_CustomDbVoid : IntegrationMigration
    {
        public override void Up()
        {
	        string s = "";
        }

        public override void Down()
        {
	        string s = "";
        }
    }
}