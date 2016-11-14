using System;
using System.IO;
using System.Threading;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure.Archiving;

namespace Experiments.ConcurrentTasks
{
    public class CreateArchiveWorker : IBackgroundWorker
    {
        private readonly IArchiveService _archive;

        public CreateArchiveWorker(IArchiveService archive)
        {
            _archive = archive;
        }

        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            var archive = _archive.ArchiveFile(new FileInfo(@"c:\tmp\Archive-478.zip"), options => options
                .ExpiresAfter(TimeSpan.FromHours(1)));

            context.Console.WriteLine("Archive {0}", archive.Id);

            return context.Wait(TimeSpan.FromSeconds(2));
        }
    }
}