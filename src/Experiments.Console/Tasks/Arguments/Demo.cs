using System;
using System.Collections.Generic;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;
using Vertica.Utilities;

namespace Experiments.Console.Tasks.Arguments
{
    public static class Demo
    {
        public static void Run(params string[] args)
        {
            args = args == null || args.Length == 0 ? new[] { nameof(MainTask), "From:\"01-06-2018\"", "To:\"01-09-2018\"" } : args;

            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .Tasks(tasks => tasks
                    .Clear()
                    .Task<MainTask>()
                    .Task<SubTask>())))
            {
                // Run based on "outside" parameters
                context.Execute(args);

                // Programatically run the "MainTask" with custom parameters
                var factory = context.Resolve<ITaskFactory>();
                var runner = context.Resolve<ITaskRunner>();

                runner.Execute(factory.Get<MainTask>(), new Vertica.Integration.Model.Arguments(
                    new KeyValuePair<string, string>("From", "01-06-2018"),
                    new KeyValuePair<string, string>("To", "01-09-2018")));
            }
        }

        public class MainTask : IntegrationTask
        {
            private readonly ITaskFactory _factory;
            private readonly ITaskRunner _runner;

            public MainTask(ITaskFactory factory, ITaskRunner runner)
            {
                _factory = factory;
                _runner = runner;
            }

            public override void StartTask(ITaskExecutionContext context)
            {
                _runner.Execute(_factory.Get<SubTask>(), context.Arguments);
            }

            public override string Description => string.Empty;
        }

        public class SubTask : IntegrationTask
        {
            private readonly IConsoleWriter _writer;

            public SubTask(IConsoleWriter writer)
            {
                _writer = writer;
            }

            public override void StartTask(ITaskExecutionContext context)
            {
                _writer.WriteLine($"Arguments: {context.Arguments}");

                if (context.Arguments.TryGetValue("From", out string from) &&
                    context.Arguments.TryGetValue("To", out string to))
                {
                    var range = new Range<DateTime>(DateTime.Parse(from), DateTime.Parse(to));

                    _writer.WriteLine($"Range: {range}");
                }
            }

            public override string Description => string.Empty;
        }
    }
}