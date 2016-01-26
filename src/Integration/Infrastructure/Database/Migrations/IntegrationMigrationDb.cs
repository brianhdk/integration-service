using System;
using System.Reflection;
using FluentMigrator.Runner;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	public class IntegrationMigrationDb : MigrationDb
	{
		internal IntegrationMigrationDb(DatabaseServer databaseServer, ConnectionString connectionString, Assembly assembly, string namespaceContainingMigrations)
			: base(databaseServer, connectionString, assembly, namespaceContainingMigrations, "IntegrationDb")
		{
		}

		public MigrationDb CopyTo(Type type, string identifyingName)
		{
			if (type == null) throw new ArgumentNullException("type");

			return new MigrationDb(DatabaseServer, ConnectionString, type.Assembly, type.Namespace, identifyingName);
		}

		public override void List(MigrationRunner runner)
		{
			// we don't expose our migrations
		}

		public override void Rollback(MigrationRunner runner)
		{
			// we don't support rollbacks
		}
	}
}