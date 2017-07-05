using System;
using System.IO;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Model;

namespace Experiments.Console.FileWatcher
{
    internal class HandleFileTask : Task
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

            try
            {
                if (file.Name.IndexOf("fail", StringComparison.OrdinalIgnoreCase) >= 0)
                    throw new InvalidOperationException($"failed {file.Name}");

                context.Log.Message($"Name: {file.Name}, Size: {file.Length}");

                // Simulate processing file takes time
                context.CancellationToken.WaitHandle.WaitOne(300);
            }
            finally
            {
                file.Delete();
            }
        }

        public override string Description => nameof(HandleFileTask);
    }
}