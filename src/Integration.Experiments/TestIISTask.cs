using System;
using Microsoft.Web.Administration;
using Vertica.Integration.IIS;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class TestIISTask : Task
    {
        private readonly IServerManagerFactory _factory;

        public TestIISTask(IServerManagerFactory factory)
        {
            _factory = factory;
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            using (ServerManager server = _factory.Create())
            {
                foreach (var site in server.Sites)
                {
                    context.Log.Message("{0} - {1}", site.Name, site.LogFile.Directory); // %SYSTEMDRIVE%
                }
            }
            // Få fat i et specifikt site
        }

        public override string Description
        {
            get { return "Tests IIS"; }
        }
    }
}