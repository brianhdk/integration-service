using System.IO;
using Examples.LiteServer.Tasks;
using Vertica.Integration.Domain.LiteServer.Servers.IO;
using Vertica.Integration.Model;

namespace Examples.LiteServer.Servers
{
    public class MonitorProductImportFolder : FileWatcherRunTaskServer<ImportProductsTask>
    {
        public MonitorProductImportFolder(ITaskFactory factory, ITaskRunner runner)
            : base(factory, runner)
        {
        }

        protected override DirectoryInfo PathToMonitor()
        {
            return new DirectoryInfo(@"c:\tmp\teammeeting\productimport");
        }

        protected override string Filter => "*.csv";
    }
}