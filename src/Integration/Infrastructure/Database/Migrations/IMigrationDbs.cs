using System.Collections.Generic;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	public interface IMigrationDbs : IEnumerable<MigrationDb>
	{
		IMigrationDbs WithIntegrationDb(IntegrationMigrationDb integrationDb);
	}
}