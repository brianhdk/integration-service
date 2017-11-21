using System;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Model;

namespace Experiments.Console.Tasks.Exceptions
{
    public static class Demo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .Tasks(tasks => tasks
                    .Task<TaskThatThrows>())))
            {
                var factory = context.Resolve<ITaskFactory>();
                var runner = context.Resolve<ITaskRunner>();

                ITask task = factory.Get<TaskThatThrows>();

                try
                {
                    runner.Execute(task);
                }
                catch (Exception ex)
                {
                    var writer = context.Resolve<IConsoleWriter>();

                    writer.WriteLine($@"
------------------------------------------------------
Message: {ex.Message}
------------------------------------------------------
StackTrace: {ex.GetFullStacktrace()}
------------------------------------------------------");
                }
            }
        }
    }

    public class TaskThatThrows : Task
    {
        public override void StartTask(ITaskExecutionContext context)
        {
            throw new InvalidOperationException("I'm throwing...");
        }

        public override string Description => nameof(TaskThatThrows);
    }
}