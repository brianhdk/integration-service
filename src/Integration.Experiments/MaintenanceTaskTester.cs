using System;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public static class MaintenanceTaskTester
    {
        public static ApplicationConfiguration TestMaintenanceTask(this ApplicationConfiguration application)
        {
            return application
                .Tasks(tasks => tasks
                    .Task<ResetMaintenanceTask>()
                    .MaintenanceTask());
        }
    }

    public class ResetMaintenanceTask : Task
    {
        private readonly ITaskRunner _runner;
        private readonly ITaskFactory _factory;
        private readonly IConfigurationService _configuration;

        public ResetMaintenanceTask(ITaskRunner runner, ITaskFactory factory, IConfigurationService configuration)
        {
            _runner = runner;
            _factory = factory;
            _configuration = configuration;
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            var configuration = _configuration.Get<MaintenanceConfiguration>();

            configuration.ArchiveFolders.Clear();
            configuration.ArchiveFolders.Enabled = true;

            configuration.ArchiveFolders.Add((folder, handlers) =>
            {
                folder.Path = @"C:\inetpub\logs\LogFiles\W3SVC13";

                folder.ArchiveOptions
                    .Named("IIS")
                    .GroupedBy("Logs")
                    .ExpiresAfterMonths(2);

                return handlers.FilesOlderThan(TimeSpan.FromDays(5));
            });

            configuration.ArchiveFolders.Add((folder, handlers) =>
            {
                folder.Path = @"\\maersk-web01\c$\Websites\stargate3\data\logs";

                folder.ArchiveOptions
                    .Named("Sitecore")
                    .GroupedBy("Logs")
                    .ExpiresAfterMonths(2);

                return handlers.FilesOlderThan(TimeSpan.FromDays(5), "*.txt");
            });

            configuration.ArchiveFolders.Add((folder, handlers) =>
            {
                folder.Path = @"C:\tmp\201506";

                folder.ArchiveOptions
                    .Named("Temp")
                    .GroupedBy("Logs")
                    .ExpiresAfterMonths(1);

                return handlers.Everything();
            });

            _configuration.Save(configuration, Name);

            _runner.Execute(_factory.Get<MaintenanceTask>());
        }

        public override string Description => "test";
    }
}