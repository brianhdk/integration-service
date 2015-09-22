using System.Collections.Generic;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	public interface IMigrationDbs : IEnumerable<MigrationDb>
	{
		bool IntegrationDbDisabled { get; }
		DatabaseServer IntegrationDbDatabaseServer { get; }
		bool CheckExistsAndCreateIntegrationDbIfNotFound { get; }

		IMigrationDbs WithIntegrationDb(MigrationDb integrationDb);
	}
}