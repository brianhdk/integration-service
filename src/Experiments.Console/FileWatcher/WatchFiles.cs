using System;
using System.IO;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Domain.LiteServer.Servers.IO;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;

namespace Experiments.Console.FileWatcher
{
    internal class WatchFiles : FileWatcherRunTaskServer<HandleFileTask>
    {
        private readonly IShutdown _shutdown;

        public WatchFiles(ITaskFactory factory, ITaskRunner runner, IShutdown shutdown) : base(factory, runner)
        {
            _shutdown = shutdown;
        }

        protected override DirectoryInfo PathToMonitor()
        {
            return new DirectoryInfo(@"c:\tmp\tomonitor");
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
                base.ProcessFile(file, args);
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
    }
}