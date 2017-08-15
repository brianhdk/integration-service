using System;
using System.Threading.Tasks;
using Vertica.Integration;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Infrastructure.Threading;
using Vertica.Integration.Infrastructure.Threading.DistributedMutex.Db;
using Vertica.Integration.Model.Exceptions;

namespace Experiments.Console.Tasks.ConcurrentExecution
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
                            // We'll wait for a maximum of 4 seconds to acquire a lock before timing out.
                            .Set("ConcurrentTaskExecution.DefaultWaitTime", "00:00:04")
                            // We'll query the DB for an available lock every 2 second.
                            .Set("DbDistributedMutex.QueryLockInterval", "00:00:02"))))
                .Tasks(tasks => tasks
                    .Task<MustRunAloneTask>()
                    .Task<CanRunInParallelTask>()
                    .ConcurrentTaskExecution(concurrentTaskExecution => concurrentTaskExecution
                        // Registers a class that overrides the default description set on a lock.
                        .AddCustomLockDescription<MyCustomLockDescription>()))))
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
                        Task.Factory.StartNew(() => safeContext[0].Execute(nameof(MustRunAloneTask), "Alias:C")),

                        // All instances should be executed in parallel
                        Task.Factory.StartNew(() => safeContext[0].Execute(nameof(CanRunInParallelTask), "Alias:A")),
                        Task.Factory.StartNew(() => safeContext[0].Execute(nameof(CanRunInParallelTask), "Alias:B"))
                    };

                    try
                    {
                        Task.WaitAll(tasks);
                    }
                    catch (AggregateException ex)
                    {
                        ex.Handle(taskException =>
                        {
                            var taskExecutionFailed = taskException as TaskExecutionFailedException;

                            var timeoutException = taskExecutionFailed?.InnerException as DistributedMutexTimeoutException;

                            if (timeoutException != null)
                            {
                                safeContext[0].Resolve<IConsoleWriter>().WriteLine(timeoutException.Message);
                                return true;
                            }

                            return false;
                        });
                    }
                    
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
    }
}