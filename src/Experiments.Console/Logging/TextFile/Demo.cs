using Vertica.Integration;
using Vertica.Integration.Infrastructure.Logging;

namespace Experiments.Console.Logging.TextFile
{
    public static class Demo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .Logging(logging => logging
                    // Register TextFile logger and configure it to organize files by daily subfolders.
                    .TextFileLogger(textFileLogger => textFileLogger
                        .OrganizeSubFoldersBy(basedOn => basedOn
                            .Daily)))
                .Services(services => services
                    .Advanced(advanced => advanced
                        // We'll override where to read RuntimeSettings from - just for demo purposes
                        // It's recommended to configure these in the app.config instead - which is the default behaviour
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            // Override where Integration Service will place log-files
                            .Set("TextLogger.BaseDirectory", @"c:\tmp\logs"))))))
            {
                var logger = context.Resolve<ILogger>();

                logger.LogWarning(Target.Custom("Order Managers"), "Some warning - e.g. about invalid data.");
                logger.LogError(Target.Service, "Some exception - that DevOps should fix.");
            }
        }
    }
}