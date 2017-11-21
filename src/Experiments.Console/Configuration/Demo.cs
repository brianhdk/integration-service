using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using FluentMigrator;
using Vertica.Integration;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Model;

namespace Experiments.Console.Configuration
{
    public static class Demo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Connection(ConnectionString.FromText(@"Integrated Security=SSPI;Data Source=.\SQLExpress;Database=IntegrationServiceDemo_Configuration"))))
                .Migration(migration => migration
                    .AddFromNamespaceOfThis<M1511294407_SetupTaskWithConfiguration>())
                .Tasks(tasks => tasks
                    .Task<TaskWithConfiguration>())))
            {
                var factory = context.Resolve<ITaskFactory>();
                var runner = context.Resolve<ITaskRunner>();

                // Ensures a functional database with the custom migration applied to it
                runner.Execute(factory.Get<MigrateTask>());

                // First execution of the task
                runner.Execute(factory.Get<TaskWithConfiguration>());

                // Second execution of the task
                runner.Execute(factory.Get<TaskWithConfiguration>());
            }
        }

        public class TaskWithConfiguration : Task
        {
            private readonly IConfigurationService _configuration;

            public TaskWithConfiguration(IConfigurationService configuration)
            {
                _configuration = configuration;
            }

            public override void StartTask(ITaskExecutionContext context)
            {
                var configuration = _configuration.Get<Configuration>();
                DateTimeOffset lastRun = configuration.LastRun;

                configuration.LastRun = DateTimeOffset.Now;
                configuration.Nested.Executions.Add(configuration.LastRun);

                context.Log.Message($"LastRun was: {lastRun}. Executions: [{string.Join(", ", configuration.Nested.Executions)}]");

                // Update the configuration
                _configuration.Save(configuration, Name);
            }

            public override string Description => nameof(TaskWithConfiguration);

            [Guid("FD62C6C1-0F58-4AE8-BA80-81A0659A78FA")]
            [Description("The configuration will be persisted in the database with this description")]
            public class Configuration
            {
                // Make sure to apply the [Guid("unique-guid")]-attribute on the class,
                // as this will serve as the unique ID in the database

                public Configuration()
                {
                    Nested = new NestedConfiguration();
                }

                public DateTimeOffset LastRun { get; set; }
                public int[] SomePrimeNumbers { get; set; }

                public NestedConfiguration Nested { get; }

                // It's always possible to add nested classes, if you want to divide the configuration into areas.
                public class NestedConfiguration
                {
                    public NestedConfiguration()
                    {
                        Executions = new List<DateTimeOffset>();
                    }

                    public List<DateTimeOffset> Executions { get; set; }
                }
            }
        }
    }

    [Migration(1511294407)]
    public class M1511294407_SetupTaskWithConfiguration : IntegrationMigration
    {
        public override void Up()
        {
            var configuration = GetConfiguration<Demo.TaskWithConfiguration.Configuration>();

            configuration.SomePrimeNumbers = new[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29 };

            SaveConfiguration(configuration);
        }
    }
}