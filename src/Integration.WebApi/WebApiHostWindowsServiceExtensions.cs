using System;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting.Handlers;

namespace Vertica.Integration.WebApi
{
	public static class WebApiHostWindowsServiceExtensions
	{
		public static void InstallWebApiHostAsWindowsService(this IntegrationMigration migration, string url = null, Action<InstallWindowsService> serviceArgs = null)
		{
			if (migration == null) throw new ArgumentNullException("migration");

			url = WebApiHost.EnsureUrl(url, migration.Resolve<IRuntimeSettings>());

			migration.InstallAsWindowsService(
				WebApiHost.Command,
				Description(url, WebApiHost.HostDescription),
				serviceArgs,
				new Arguments(WebApiHost.WithUrl(url)));
		}

		public static void UninstallWebApiHostWindowsService(this IntegrationMigration migration)
		{
			migration.UninstallWindowsService(WebApiHost.Command);
		}

		internal static string WindowsServiceDescription(this WebApiHost host, string url)
		{
			if (host == null) throw new ArgumentNullException("host");

			return Description(url, host.Description);
		}

		private static string Description(string url, string description)
		{
			return String.Format("[URL: {0}] {1}", url, description);
		}
	}
}