using System.Linq;
using Vertica.Integration;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Model;
using Task = System.Threading.Tasks.Task;

namespace Experiments.ConcurrentTasks
{
    class Program
    {
        static void Main(string[] args)
        {
            using (IApplicationContext context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(ConnectionString.FromText("Server=.\\SQLExpress;Database=IS_ConcurrentTasks;Trusted_Connection=True;")))
                .Tasks(tasks => tasks
                    .AddFromAssemblyOfThis<Program>()
                    .MaintenanceTask())))
            {
                var factory = context.Resolve<ITaskFactory>();
                var runner = context.Resolve<ITaskRunner>();

                // migrate first
                runner.Execute(factory.Get<MigrateTask>());

                Task[] runners = Enumerable.Range(0, 4).Select(x => Task.Factory.StartNew(() =>
                {
                    ITask task = x == 0 ? factory.Get<ConcurrentExecutableTask>() : factory.Get<SynchronousOnlyTask>();

                    // run some task
                    runner.Execute(task);

                })).ToArray();

                Task.WaitAll(runners);

                // maintenance last
                runner.Execute(factory.Get<MaintenanceTask>());
            }
        }
    }
}
