using System;
using System.Collections.Generic;
using System.Linq;
using Vertica.Integration;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Threading.DistributedMutex.Db;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Experiments.Console.Tasks.ConcurrentExecution.ExceptionHandler
{
    public static class Demo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .PrefixTables("IntegrationDb_")
                        .Connection(ConnectionString.FromText(@"Integrated Security=SSPI;Data Source=.\SQLExpress;Database=IntegrationService_Tasks"))))
                .Services(services => services
                    .Advanced(advanced => advanced
                        // We'll override where to read RuntimeSettings from - just for demo purposes
                        // It's recommended to configure these in the app.config instead - which is the default behaviour
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            .Set("ConcurrentTaskExecution.DefaultWaitTime", "00:00:10")
                            .Set("DbDistributedMutex.QueryLockInterval", "00:00:02"))))
                .Tasks(tasks => tasks
                    .Task<MustRunAloneTask>()
                    .ConcurrentTaskExecution(concurrentTaskExecution => concurrentTaskExecution
                        // Registers a class that overrides the default behaviour for exceptions
                        .AddExceptionHandler<MyExceptionHandler>()))))
            {
                // Ensure up-to-date db
                context.Execute(nameof(MigrateTask));

                try
                {
                    IApplicationContext[] safeContext = { context };

                    Task[] tasks =
                    {
                        // Only one instance at the time will be allowed to be executed - and one of them will timeout
                        Task.Factory.StartNew(() => safeContext[0].Execute(nameof(MustRunAloneTask), "Alias:A")),
                        Task.Factory.StartNew(() => safeContext[0].Execute(nameof(MustRunAloneTask), "Alias:B")),
                        Task.Factory.StartNew(() => safeContext[0].Execute(nameof(MustRunAloneTask), "Alias:C"))
                    };

                    // One task will run for 5 seconds before releasing the lock
                    // The two other tasks will wait for a maximum of 10 seconds to acquire the lock before timing out
                    // One of the remaining two tasks will acquire the lock for 5 seconds
                    // One task will not be able to acquire the lock within before timing out
                    Task.WaitAll(tasks);
                }
                finally
                {
                    // We'll clean-up all locks - so this demo will work every time :)
                    int deletions = context
                        .Resolve<IDeleteDbDistributedMutexLocksCommand>()
                        .Execute(DateTimeOffset.UtcNow);

                    if (deletions > 0)
                        context.Resolve<IConsoleWriter>().WriteLine("Deleted {0} lock(s).", deletions);
                }
            }
        }

        [PreventConcurrentTaskExecution(ExceptionHandler = typeof(MyExceptionHandler))]
        public class MustRunAloneTask : IntegrationTask
        {
            private readonly HashSet<string> _isRunning;

            public MustRunAloneTask()
            {
                _isRunning = new HashSet<string>();   
            }

            public override void StartTask(ITaskExecutionContext context)
            {
                string alias = context.Arguments["Alias"];

                lock (_isRunning)
                {
                    if (_isRunning.Count != 0)
                        throw new InvalidOperationException($"Unable to start {alias}. Multiple instances running: {string.Join(", ", _isRunning.ToArray())}");

                    _isRunning.Add(alias);
                }
                
                context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(5));
                context.Log.Message("Alias {0} finished...", alias);
            }

            public override void End(ITaskExecutionContext<EmptyWorkItem> context)
            {
                string alias = context.Arguments["Alias"];

                lock (_isRunning)
                {
                    _isRunning.Remove(alias);

                    if (_isRunning.Count != 0)
                        throw new InvalidOperationException($"Unable to end {alias}. Multiple instances running: {string.Join(", ", _isRunning.ToArray())}");
                }

                base.End(context);
            }

            public override string Description => nameof(MustRunAloneTask);
        }

        public class MyExceptionHandler : IPreventConcurrentTaskExecutionExceptionHandler
        {
            public Exception OnException(ITask currentTask, TaskLog log, Vertica.Integration.Model.Arguments arguments, Exception exception)
            {
                string alias = arguments["Alias"];

                log.LogMessage($"Unable to start {alias} (TaskLogId: {log.Id}): {exception.Message}");

                // Return <null> to stop the task from being executed but without any exceptions thrown

                return null;
            }
        }
    }
}