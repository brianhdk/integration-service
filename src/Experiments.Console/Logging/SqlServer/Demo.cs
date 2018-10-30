using System;
using Vertica.Integration;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.Email;
using Vertica.Integration.Infrastructure.IO;
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
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IEmailService, MyEmailService>()))
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .PrefixTables("IntegrationService.")
                        .Connection(ConnectionString.FromText(@"Integrated Security=SSPI;Data Source=.\SQLExpress;Database=IntegrationServiceDemo_Logging"))))))
            {
                context.Execute(nameof(MigrateTask));

                var logger = context.Resolve<ILogger>();

                logger.LogError(Target.Service, new string('-', 5000));
                logger.LogError(new InvalidOperationException(new string('.', 5000)));

                var configurationService = context.Resolve<IConfigurationService>();
                var monitorConfiguration = configurationService.Get<MonitorConfiguration>();

                MonitorTarget serviceTarget = monitorConfiguration.EnsureMonitorTarget(Target.Service);
                serviceTarget.Recipients = new[] {"dummy@localhost"};

                configurationService.Save(monitorConfiguration, typeof(Demo).FullName);
                
                context.Execute(nameof(MonitorTask));
            }
        }

        public class MyEmailService : IEmailService
        {
            private readonly IConsoleWriter _console;

            public MyEmailService(IConsoleWriter console)
            {
                _console = console;
            }

            public void Send(EmailTemplate template, params string[] recipients)
            {
                _console.WriteLine(template.GetBody());
            }
        }
    }
}