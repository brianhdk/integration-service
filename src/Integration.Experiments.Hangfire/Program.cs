using System;
using Hangfire;
using Hangfire.SqlServer;
using Vertica.Integration;
using Vertica.Integration.Hangfire;
using Vertica.Integration.Infrastructure.Database.Migrations;

namespace Integration.Experiments.Hangfire
{
	class Program
	{
		static void Main(string[] args)
		{
			using (IApplicationContext context = ApplicationContext.Create(application => application
				.UseHangfire(hangfire => hangfire
					.Configuration(configuration => configuration
						.UseSqlServerStorage("IntegrationDb", new SqlServerStorageOptions
						{
							QueuePollInterval = TimeSpan.FromSeconds(1)
						})
					)
					.OnStartup(startup => startup.RunMigrateTask())
					.AddFromAssemblyOfThis<Program>())
				.Database(database => database.DisableIntegrationDb())
				.Tasks(tasks => tasks.Clear().Task<HangfireTask>().Task<MigrateTask>())))
			{
				context.Execute(args);
			}
		}
	}
}
