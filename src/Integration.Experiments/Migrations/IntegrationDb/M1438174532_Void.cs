using FluentMigrator;
using Vertica.Integration.Infrastructure.Database.Migrations;

namespace Vertica.Integration.Experiments.Migrations.IntegrationDb
{
	[Migration(1438174532)]
    public class M1438174532_Void : IntegrationMigration
    {
        public override void Up()
        {
	        ApplicationEnvironment environment = RuntimeSettings.Environment;

	        if (environment == ApplicationEnvironment.Development)
	        {
				string s = "";
	        }
	        else
	        {
		        string s = "";
	        }
        }

        public override void Down()
        {
        }
    }
}