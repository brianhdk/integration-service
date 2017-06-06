using System;
using FluentMigrator;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Model;
using Vertica.Integration.UCommerce.Maintenance;

namespace Vertica.Integration.UCommerce
{
	public static class UCommerceExtensions
	{
		public static ApplicationConfiguration UseUCommerce(this ApplicationConfiguration application, Action<UCommerceConfiguration> uCommerce = null)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			return application.Extensibility(extensibility =>
			{
				UCommerceConfiguration configuration = extensibility.Register(() => new UCommerceConfiguration(application));

				uCommerce?.Invoke(configuration);
			});
		}

        public static TaskConfiguration<MaintenanceWorkItem> IncludeUCommerce(this TaskConfiguration<MaintenanceWorkItem> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            task.Application.UseUCommerce();

            return task
                .Step<UCommerceIndexMaintenanceStep>();
        }

        public static MigrationConfiguration AddUCommerceFromNamespaceOfThis<T>(this MigrationConfiguration migration, DatabaseServer db, string identifyingName = null)
            where T : Migration
        {
            if (migration == null) throw new ArgumentNullException(nameof(migration));

            migration.Application
                .UseUCommerce(uCommerce => uCommerce
                    .AddMigrationFromNamespaceOfThis<T>(db, identifyingName));

            return migration;
        }
    }
}