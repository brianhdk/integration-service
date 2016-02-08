using System;
using Hangfire;
using Hangfire.SqlServer;
using Integration.Hangfire;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Extensions;

namespace Integration.Experiments.Hangfire
{
	class Program
	{
		static void Main()
		{
			// TODO: Make this part of the Hangfire configuration
			GlobalConfiguration.Configuration
				.UseSqlServerStorage("IntegrationDb", new SqlServerStorageOptions
				{
					QueuePollInterval = TimeSpan.FromSeconds(1)
				});

			using (IApplicationContext context = ApplicationContext.Create(application => application
				.UseHangfire(hangfire => hangfire
					.DisableDashboard()
					.AddFromAssemblyOfThis<Program>())
				.Database(database => database.DisableIntegrationDb())
				.Tasks(tasks => tasks.Clear().Task<HangfireTask>())))
			{
				context.Execute(typeof(HangfireHost).HostName());
			}
		}
	}
}
