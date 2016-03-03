using System;
using System.IO;
using Hangfire;
using Hangfire.Example.Shared;
using Hangfire.SqlServer;
using Integration.Experiments.Hangfire.Migrations;
using Vertica.Integration;
using Vertica.Integration.Hangfire;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Model;

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
				.Migration(migration => migration.AddFromNamespaceOfThis<M1456825440_HangfireScheduledTask>())
				.Tasks(tasks => tasks.AddFromAssemblyOfThis<ExportCatalogTask>())
				.AddCustomInstaller(Install.Service<ISomeService, SomeService>())
				.AddCustomInstaller(Install.Service<ITaskByNameRunner, TaskByNameRunner>())
				.AddCustomInstaller(Install.ByConvention.AddFromAssemblyOfThis<Program>())))
			{
				context.Execute(args);
			}
		}
	}

	public class ExportCatalogTask : Task
	{
		public override string Description => "TBD";

		public override void StartTask(ITaskExecutionContext context)
		{
			context.Log.Message("Hello!");
		}
	}

	public class SomeService : ISomeService
	{
		private readonly TextWriter _outputter;

		public SomeService(TextWriter outputter)
		{
			_outputter = outputter;
		}

		public void Execute(string message)
		{
			_outputter.WriteLine(message);
		}
	}

	public class TaskByNameRunner : ITaskByNameRunner
	{
		readonly ITaskRunner _runner;
		readonly ITaskFactory _factory;

		public TaskByNameRunner(ITaskRunner runner, ITaskFactory factory)
		{
			_runner = runner;
			_factory = factory;
		}

		public void Run(string taskName)
		{
			_runner.Execute(_factory.Get(taskName));
		}
	}
}