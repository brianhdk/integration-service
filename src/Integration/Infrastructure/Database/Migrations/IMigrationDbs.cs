using System.Collections.Generic;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	public interface IMigrationDbs : IEnumerable<MigrationDb>
	{
		DatabaseServer IntegrationDbDatabaseServer { get; }
		bool CheckExistsAndCreateIntegrationDbIfNotFound { get; }

		IMigrationDbs WithIntegrationDb(IntegrationMigrationDb integrationDb);
	}
}