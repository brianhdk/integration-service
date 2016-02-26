using FluentMigrator.VersionTableInfo;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    [VersionTableMetaData]
    public class VersionTable : DefaultVersionTableMetaData
    {
        public override string TableName => "BuiltIn_" + base.TableName;
    }
}