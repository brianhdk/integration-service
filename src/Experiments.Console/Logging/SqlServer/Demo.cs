using System;
using Vertica.Integration;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.Logging;

namespace Experiments.Console.Logging.SqlServer
{
    public static class Demo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Tasks(tasks => tasks
                    .Clear()
                    .Task<MigrateTask>()
                    .MonitorTask(task => task
                        .Clear()
                        .Step<ExportIntegrationErrorsStep>()))
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .PrefixTables("IntegrationService.")
                        .Connection(ConnectionString.FromText(@"Integrated Security=SSPI;Data Source=.\SQLExpress;Database=IntegrationServiceDemo_Logging"))))))
            {
                context.Execute("MigrateTask");

                var logger = context.Resolve<ILogger>();

                logger.LogError(Target.Service, new string('-', 5000));
                logger.LogError(new InvalidOperationException(new string('.', 5000)));

                context.Execute("MonitorTask");
            }
        }
    }
}