using System;
using System.IO;
using System.Threading;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Domain.LiteServer.Servers.IO;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Task = System.Threading.Tasks.Task;

namespace Experiments.Console.FileWatcher.LargeFiles
{
    public static class Demo
    {
        public static void Run()
        {
            var directory = new DirectoryInfo("c:\\tmp\\filewatcher.demo\\");
            directory.Create();
            
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            .Set("Directory", directory.FullName))))
                .Tasks(tasks => tasks
                    .Task<HandleFileTask>())
                .UseLiteServer(liteServer => liteServer
                    .AddServer<WatchFiles>()
                    .AddServer<GenerateFiles>()
                    .HouseKeeping(houseKeeping => houseKeeping
                        .Interval(TimeSpan.FromSeconds(10))
                        .OutputStatusOnNumberOfIterations(12)))))
            {
                context.Execute(nameof(LiteServerHost));
            }
        }

        public class GenerateFiles : IBackgroundServer
        {
            private readonly IRuntimeSettings _settings;

            public GenerateFiles(IRuntimeSettings settings)
            {
                _settings = settings;
            }

            public Task Create(BackgroundServerContext context, CancellationToken token)
            {
                return Task.Factory.StartNew(() =>
                {
                    int filesGenerated = 0;

                    while (!token.IsCancellationRequested)
                    {
                        string fileName = Path.Combine(_settings["Directory"], $"{Path.GetRandomFileName()}.psd");

                        var file = new FileInfo(fileName);

                        using (StreamWriter writer = file.CreateText())
                        {
                            const int oneMegaByte = 1024 * 1024;

                            writer.WriteAsync(new string('A', oneMegaByte)).Wait(token);
                        }

                        if (++filesGenerated > 200)
                            break;

                        //token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
                    }

                }, token);
            }
        }

        internal class WatchFiles : FileWatcherRunTaskServer<HandleFileTask>
        {
            private readonly IRuntimeSettings _settings;
            private readonly IShutdown _shutdown;

            public WatchFiles(ITaskFactory factory, ITaskRunner runner, IRuntimeSettings settings, IShutdown shutdown)
                : base(factory, runner)
            {
                _settings = settings;
                _shutdown = shutdown;
            }

            protected override void ProcessFile(FileInfo file, FileSystemEventArgs args)
            {
                // This will process one file at the time

                file = EnsureFile(file);

                if (file != null)
                {
                    try
                    {
                        base.ProcessFile(file, args);

                        file.Delete();
                    }
                    catch (Exception ex)
                    {
                        System.Console.Error.WriteLine(ex);
                    }
                }
            }
            
            private FileInfo EnsureFile(FileInfo file)
            {
                int attempts = 10;

                while (--attempts > 0)
                {
                    if (!file.Exists)
                        return null;

                    if (!file.IsLocked())
                        return file;

                    // Waits up to 10 seconds before giving up...
                    _shutdown.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                }

                return null;
            }

            protected override DirectoryInfo PathToMonitor()
            {
                return new DirectoryInfo(_settings["Directory"]);
            }
        }

        internal class HandleFileTask : IntegrationTask
        {
            public override void StartTask(ITaskExecutionContext context)
            {
                context.Log.Message(context.Arguments["File"]);
                context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
            }

            public override string Description => "";
        }
    }
}