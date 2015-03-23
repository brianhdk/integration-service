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

            byte[] archive = _archiver.Get(arguments[0]);

            if (archive != null)
                File.WriteAllBytes(String.Format("Archive-{0}.zip", arguments[0]), archive);
        }
    }
}