using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Builders.Create.Column;
using FluentMigrator.Builders.Create.Table;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    public static class MigrationExtensions
    {
        public static ICreateTableColumnOptionOrWithColumnSyntax AsDateTimeOffset(this ICreateTableColumnAsTypeSyntax create)
        {
            return create.AsCustom("DateTimeOffset");
        }

        public static ICreateColumnOptionSyntax AsDateTimeOffset(this ICreateColumnAsTypeOrInSchemaSyntax create)
        {
            return create.AsCustom("DateTimeOffset");
        }

        public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax AsDateTimeOffset(this IAlterTableColumnAsTypeSyntax alter)
        {
            return alter.AsCustom("DateTimeOffset");
        }
    }
}