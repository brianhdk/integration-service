using System;
using FluentMigrator;
using Vertica.Integration.Domain.Core;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    [Migration(6)]
    public class M6_ExtendArchiveWithExpirationAndGroup : IntegrationMigration
    {
        public override void Up()
        {
            Alter.Table("Archive")
                .AddColumn("Expires").AsDateTimeOffset().Nullable()
                .AddColumn("GroupName").AsString().Nullable();

            var configuration = GetConfiguration<MaintenanceConfiguration>();
#pragma warning disable 618
            var days = (int) configuration.CleanUpArchivesOlderThan.TotalDays;
#pragma warning restore 618

            Execute.Sql("UPDATE Archive SET Expires = DATEADD(d, " + days + ", Created)");
        }

        public override void Down()
        {
            throw new NotSupportedException("Migrating down is not supported.");
        }
    }
}