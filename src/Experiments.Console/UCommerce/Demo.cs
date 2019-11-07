using Vertica.Integration;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.UCommerce;

namespace Experiments.Console.UCommerce
{
    public static class Demo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .UseUCommerce(uCommerce => uCommerce
                    .Connection(ConnectionString.FromText(@"Integrated Security=SSPI;Data Source=.\SQLExpress;Database=Hoka_Sitecore_uCommerce")))
                .Tasks(tasks => tasks
                    .Clear()
                    .MaintenanceTask(task => task
                        .Clear()
                        .IncludeUCommerce()))))
            {
                context.Execute(nameof(MaintenanceTask));
            }
        }
    }
}