using System;
using System.Threading;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Model;

namespace Experiments.ConcurrentTasks
{
    public class RunMaintenanceTask : IBackgroundWorker
    {
        private readonly ITaskRunner _taskRunner;
        private readonly ITaskFactory _taskFactory;

        public RunMaintenanceTask(ITaskRunner taskRunner, ITaskFactory taskFactory, IConfigurationService configuration)
        {
            _taskRunner = taskRunner;
            _taskFactory = taskFactory;

            var maintenanceConfiguration = configuration.Get<MaintenanceConfiguration>();

            maintenanceConfiguration.ArchiveFolders.AddOrUpdate("UmbracoLogFiles", (folder, handler) =>
            {
                folder.Path = @"c:\\tmp";
                folder.ArchiveOptions.ExpiresAfter(TimeSpan.FromDays(365));

                return handler.FilesOlderThan(TimeSpan.FromDays(-1), "b.txt");
            });

            configuration.Save(maintenanceConfiguration, nameof(RunMaintenanceTask));
        }

        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            //if (context.InvocationCount == 1)
            //    return context.Wait(TimeSpan.FromSeconds(30));

            _taskRunner.Execute(_taskFactory.Get<MaintenanceTask>());

            return context.Wait(TimeSpan.FromMinutes(5));
        }
    }
}