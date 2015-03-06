using System;
using FluentMigrator;

namespace Vertica.Integration.Experiments.Custom_Database.Migrations
{
    [Migration(2)]
    public class M2_Test : Migration
    {
        public override void Up()
        {
            Alter.Table("Test").AddColumn("Name").AsString(50);
        }

        public override void Down()
        {
            throw new NotSupportedException(@"Not supported.");
        }
    }
}