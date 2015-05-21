using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class FileSystemTesterTask : Task
    {
        private readonly IFileSystemService _fileSystem;

        public FileSystemTesterTask(IFileSystemService fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string Description
        {
            get { return "TBD"; }
        }
    }
}