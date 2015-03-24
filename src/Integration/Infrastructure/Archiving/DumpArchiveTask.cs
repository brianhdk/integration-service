using System;
using System.IO;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public class DumpArchiveTask : Task
    {
        private readonly IArchiver _archiver;

        public DumpArchiveTask(IArchiver archiver)
        {
            _archiver = archiver;
        }

        public override string Description
        {
            get { return "Dumps a specified archive to the file system."; }
        }

        public override void StartTask(Log log, params string[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
                throw new InvalidOperationException(@"Parameter usage: [ArchiveID]");

            string id = arguments[0];

            byte[] archive = _archiver.Get(id);

            if (archive != null)
            {
                DirectoryInfo directory = Directory.CreateDirectory("Archive-Dumps");

                var file = Path.Combine(directory.FullName, String.Format("Archive-{0}.zip", id));

                File.WriteAllBytes(file, archive);

                log.Message("Archive dumped to {0}", file);
            }
            else
            {
                log.Warning("Archive '{0}' not found.", id);
            }
        }
    }
}