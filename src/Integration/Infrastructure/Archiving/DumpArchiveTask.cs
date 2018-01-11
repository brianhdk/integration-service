using System.IO;
using System.Linq;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Archiving
{
#pragma warning disable 618
    public class DumpArchiveTask : Task
#pragma warning restore 618
    {
        private readonly IArchiveService _archive;

        public DumpArchiveTask(IArchiveService archive)
        {
            _archive = archive;
        }

        public override string Description => "Dumps a specified archive to the file system.";

	    public override void StartTask(ITaskExecutionContext context)
        {
            foreach (string id in context.Arguments.Select(x => x.Key))
            {
                byte[] archive = _archive.Get(id);

                if (archive != null)
                {
                    DirectoryInfo directory = Directory.CreateDirectory("Archive-Dumps");

                    string file = Path.Combine(directory.FullName, $"{id}.zip");

                    File.WriteAllBytes(file, archive);

                    context.Log.Message("Archive dumped to {0}.", file);
                }
                else
                {
                    context.Log.Warning(Target.Service, "Archive '{0}' not found.", id);
                }                
            }
        }
    }
}