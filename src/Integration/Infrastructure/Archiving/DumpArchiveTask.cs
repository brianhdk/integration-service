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
            get { return "Dumps a specified archive-file to the disc."; }
        }

        public override string Schedule
        {
            get { return "Manual"; }
        }

        public override void StartTask(Log log, params string[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
                throw new InvalidOperationException(@"Parameter usage: [ArchiveID]");

            uint archiveId;
            if (!UInt32.TryParse(arguments[0], out archiveId))
                throw new InvalidOperationException(@"Invalid first parameter, should be an int.");

            byte[] archive = _archiver.Get((int)archiveId);

            if (archive != null)
                File.WriteAllBytes(String.Format("Archive-{0}.zip", archiveId), archive);
        }
    }
}