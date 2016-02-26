using System;
using System.Reflection;
using Castle.MicroKernel;
using FluentMigrator.Runner;
using Vertica.Integration.Model;

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
			if (type == null) throw new ArgumentNullException(nameof(type));

			return new MigrationDb(DatabaseServer, ConnectionString, type.Assembly, type.Namespace, identifyingName);
		}

		public override void List(MigrationRunner runner, ITaskExecutionContext context, IKernel kernel)
		{
			// we don't expose our migrations
		}

		public override void Rollback(MigrationRunner runner, ITaskExecutionContext log, IKernel kernel)
		{
			// we don't support rollbacks
		}
	}
}