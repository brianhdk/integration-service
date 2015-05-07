using System;
using System.IO;
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

        public override void StartTask(ILog log, params string[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
                throw new InvalidOperationException(@"Parameter usage: [ArchiveID]");

            string id = arguments[0];

            byte[] archive = _archive.Get(id);

            if (archive != null)
            {
                DirectoryInfo directory = Directory.CreateDirectory("Archive-Dumps");

                string file = Path.Combine(directory.FullName, String.Format("{0}.zip", id));

                File.WriteAllBytes(file, archive);

                log.Message("Archive dumped to {0}.", file);
            }
            else
            {
                log.Warning("Archive '{0}' not found.", id);
            }
        }
    }
}