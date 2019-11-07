using System;
using System.IO;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Domain.LiteServer.Servers.IO;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Model;
using Vertica.Utilities;

namespace Experiments.Console.FileWatcher
{
    public static class Demo
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

        internal class HandleFileTask : IntegrationTask
        {
            private readonly IArchiveService _archiveService;

            public HandleFileTask(IArchiveService archiveService)
            {
                _archiveService = archiveService;
            }

            public override void StartTask(ITaskExecutionContext context)
            {
                FileInfo file = new FileInfo(context.Arguments["File"]);

                if (!file.Exists)
                    return;

                context.Log.Message("Archive: {0}", _archiveService.ArchiveFile(file));

                if (file.Name.IndexOf("fail", StringComparison.OrdinalIgnoreCase) >= 0)
                    throw new InvalidOperationException($"failed {file.Name}");

                context.Log.Message($"Name: {file.Name}, Size: {file.Length}");

                // Simulate processing file takes time
                context.CancellationToken.WaitHandle.WaitOne(10);
            }

            public override string Description => nameof(HandleFileTask);
        }

        internal class WatchFiles : FileWatcherRunTaskServer<HandleFileTask>
        {
            private readonly IShutdown _shutdown;
            private readonly IConsoleWriter _writer;

            public WatchFiles(ITaskFactory factory, ITaskRunner runner, IShutdown shutdown, IConsoleWriter writer)
                : base(factory, runner)
            {
                _shutdown = shutdown;
                _writer = writer;
            }

            protected override DirectoryInfo PathToMonitor()
            {
                var path = new DirectoryInfo(@"c:\tmp\tomonitor");

                foreach (string subDirectory in Enum.GetNames(typeof(SubDirectory)))
                    path.CreateSubdirectory(subDirectory);

                return path;
            }

            public override bool ShouldRestart(RestartableContext context)
            {
                if (context.FailedCount < 10)
                    return true;

                return false;
            }

            protected override string Filter => "*.txt";

            protected override void ProcessFile(FileInfo file, FileSystemEventArgs args)
            {
                file = EnsureFile(file);

                if (file != null)
                {
                    try
                    {
                        _writer.WriteLine("Processing: {0}", file.Name);

                        base.ProcessFile(file, args);

                        _writer.WriteLine("Processed: {0}", file.Name);

                        MoveTo(file, SubDirectory.Succeded);
                    }
                    catch (Exception ex)
                    {
                        _writer.WriteLine("Error processing {0} - {1}", file.Name, ex.Message);

                        MoveTo(file, SubDirectory.Failed);
                    }
                }
            }

            private static void MoveTo(FileInfo file, SubDirectory subDirectory)
            {
                file.MoveTo(Path.Combine(file.DirectoryName ?? ".", subDirectory.ToString(), $"{Time.UtcNow:yyyyMMddhhmmss}_{file.Name}"));
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

            private enum SubDirectory
            {
                Failed,
                Succeded
            }
        }
    }
}