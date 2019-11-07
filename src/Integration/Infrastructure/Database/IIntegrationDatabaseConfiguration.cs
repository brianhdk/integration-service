using Vertica.Integration.Infrastructure.Database.Migrations;

namespace Vertica.Integration.Infrastructure.Database
{
    public interface IIntegrationDatabaseConfiguration
    {
        /// <summary>
        ///     Returns true, if Integration Database has been disabled.
        /// </summary>
        bool Disabled { get; }

        /// <summary>
        ///     Returns the final table name for integration db tables - with optional prefix added.
        /// </summary>
        string TableName(IntegrationDbTable table);

        DatabaseServer DatabaseServer { get; }
        bool CheckExistsAndCreateDatabaseIfNotFound { get; }
    }
}