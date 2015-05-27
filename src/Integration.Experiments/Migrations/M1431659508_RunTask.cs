using FluentMigrator;
using Vertica.Integration.Infrastructure.Database.Migrations;

namespace Vertica.Integration.Experiments.Migrations
{
    [Migration(1431659508)]
    public class M1431659508_RunTask : IntegrationMigration
    {
        public override void Up()
        {
            RunTask<HelloTask>();
        }

        public override void Down()
        {
        }
    }
}