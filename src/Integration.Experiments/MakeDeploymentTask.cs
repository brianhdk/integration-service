using System.Collections.Generic;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class MakeDeploymentTask : Task<MakeDeploymentWorkItem>
    {
        public MakeDeploymentTask(IEnumerable<IStep<MakeDeploymentWorkItem>> steps)
            : base(steps)
        {
        }

        public override MakeDeploymentWorkItem Start(ITaskExecutionContext context)
        {
            var workItem = new MakeDeploymentWorkItem();
            workItem.AddFolder("Integration Service");

            return workItem;
        }

        public override void End(MakeDeploymentWorkItem workItem, ITaskExecutionContext context)
        {
            // ... Zip folders and files in WorkItem.
        }

        public override string Description
        {
            get { return "Creates zip-file for deployment of the solution."; }
        }
    }

    public class MakeDeploymentWorkItem
    {
        public void AddFolder(string folder)
        {
            // ...
        }

        public void AddFile(string file)
        {
            // ...
        }
    }

    public class CopyWebsiteArtifacts : Step<MakeDeploymentWorkItem>
    {
        public override void Execute(MakeDeploymentWorkItem workItem, ITaskExecutionContext context)
        {
            workItem.AddFolder("../src/Portal.Website");
        }

        public override string Description
        {
            get { return "TBD"; }
        }
    }

    public class CopySitecoreBaseline : Step<MakeDeploymentWorkItem>
    {
        public override void Execute(MakeDeploymentWorkItem workItem, ITaskExecutionContext context)
        {
            workItem.AddFile("../installs/Sitecore 8.0 rev. 141212.zip");
        }

        public override string Description
        {
            get { return "TBD"; }
        }
    }

    public class CopyUCommerceBaseline : Step<MakeDeploymentWorkItem>
    {
        public override void Execute(MakeDeploymentWorkItem workItem, ITaskExecutionContext context)
        {
            workItem.AddFile("../installs/uCommerce-for-Sitecore-6.6.6.15140.zip");
        }

        public override string Description
        {
            get { return "Copies uCommerce artifacts, including base ."; }
        }
    }
}