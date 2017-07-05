using System;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;

namespace Experiments.Console.FileWatcher
{
    public static class FileWatcherDemo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Tasks(tasks => tasks
                    .Task<HandleFileTask>())
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .UseLiteServer(liteServer => liteServer
                    .AddServer<WatchFiles>()
                    .HouseKeeping(houseKeeping => houseKeeping
                        .Interval(TimeSpan.FromSeconds(1))
                        .OutputStatusOnNumberOfIterations(10)))))
            {
                context.Execute(nameof(LiteServerHost));
            }
        }
    }
}