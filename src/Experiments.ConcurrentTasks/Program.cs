using System;
using Hangfire;
using Hangfire.SqlServer;
using Vertica.Integration;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Hangfire;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Logging.Elmah;
using Vertica.Integration.Portal;
using Vertica.Integration.UCommerce;
using Vertica.Integration.WebApi;

namespace Experiments.ConcurrentTasks
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = ConnectionString.FromText("Server=.\\SQLExpress;Database=IS_ConcurrentTasks;Trusted_Connection=True;");

            using (IApplicationContext context = ApplicationContext.Create(application => application
                .Tasks(tasks => tasks
                    .AddFromAssemblyOfThis<Program>()
                    .ConcurrentTaskExecution(concurrentTaskExecution =>
                        concurrentTaskExecution.AddFromAssemblyOfThis<Program>()))))
            {
            }
        }
    }
}