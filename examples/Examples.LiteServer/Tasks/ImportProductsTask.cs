using System;
using System.IO;
using System.Linq;
using System.Threading;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Tasks;
using Vertica.Integration.Model.Tasks.Evaluators;

namespace Examples.LiteServer.Tasks
{
    [PreventConcurrentTaskExecution]
    public class ImportProductsTask : Task
    {
        private readonly IArchiveService _archive;

        public ImportProductsTask(IArchiveService archive)
        {
            _archive = archive;
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            var file = new FileInfo(context.Arguments["File"]);

            if (!file.Exists)
                throw new FileNotFoundException($"File '{file.FullName}' not found.", file.FullName);

            ArchiveCreated archive = _archive.ArchiveFile(file);

            context.Log.Message($"Processing {file.FullName}. Archive: {archive}.");

            foreach (int product in Enumerable.Range(1, 20))
            {
                // watch for cancellation
                context.ThrowIfCancelled();

                context.Log.Message($"Importing {product} from {file.FullName}.");

                // importing products is really slow - thus we sleep for a second
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            if (!context.Arguments.Contains("SkipDelete"))
                file.Delete();

            context.Log.Message($"Done processing {file.FullName}.");
        }

        public override string Description => "Importing products.";
    }
}