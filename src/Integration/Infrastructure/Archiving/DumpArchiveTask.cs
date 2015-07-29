using System;
using System.IO;
using System.Linq;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public class DumpArchiveTask : Task
    {
        private readonly IArchiveService _archive;

        public DumpArchiveTask(IArchiveService archive)
        {
            _archive = archive;
        }

        public override string Description
        {
            get { return "Dumps a specified archive to the file system."; }
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            foreach (string id in context.Arguments.Select(x => x.Key))
            {
                byte[] archive = _archive.Get(id);

                if (archive != null)
                {
                    DirectoryInfo directory = Directory.CreateDirectory("Archive-Dumps");

                    string file = Path.Combine(directory.FullName, String.Format("{0}.zip", id));

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