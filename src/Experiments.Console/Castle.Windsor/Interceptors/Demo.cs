using System;
using System.IO;
using System.Linq;
using Castle.DynamicProxy;
using Vertica.Integration;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Model;

namespace Experiments.Console.Castle.Windsor.Interceptors
{
    public static class Demo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Connection(ConnectionString.FromText(@"Integrated Security=SSPI;Data Source=.\SQLExpress;Database=IntegrationServiceDemo_Interceptors"))))
                .Services(services => services
                    .Interceptors(interceptors => interceptors
                        .InterceptService<IConsoleWriter, ConsoleWriterInterceptor>()))
                .Tasks(tasks => tasks
                    .Task<MyTask>())))
            {
                var factory = context.Resolve<ITaskFactory>();
                var runner = context.Resolve<ITaskRunner>();

                // Execute migrations to get the database up and running
                runner.Execute(factory.Get<MigrateTask>());

                // Run the custom task
                runner.Execute(factory.Get<MyTask>());
            }
        }
    }

    internal class ConsoleWriterInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            var message = (string)invocation.Arguments.FirstOrDefault();

            if (message != null)
            {
                var args = (object[]) invocation.Arguments.ElementAtOrDefault(1);

                if (args != null && args.Length > 0)
                    message = string.Format(message, args);

                File.AppendAllText(@"c:\tmp\integrationservice-interceptor-demo.txt", $@"{message}{Environment.NewLine}");
            }
        }
    }

    public class MyTask : Task
    {
        public override void StartTask(ITaskExecutionContext context)
        {
            context.Log.Message("Hello from MyTask");
        }

        public override string Description => nameof(MyTask);
    }
}