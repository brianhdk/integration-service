using System;
using System.IO;
using System.Linq;
using System.Text;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Domain.Monitoring
{
    public class MonitorFoldersStep : Step<MonitorWorkItem>
    {
        public override Execution ContinueWith(MonitorWorkItem workItem)
        {
            if (workItem.Configuration.MonitorFolders.GetEnabledFolders().Length == 0)
                return Execution.StepOver;

            return Execution.Execute;
        }

        public override void Execute(MonitorWorkItem workItem, ITaskExecutionContext context)
        {
            MonitorConfiguration.MonitorFoldersConfiguration.Folder[] folders =
                workItem.Configuration.MonitorFolders.GetEnabledFolders();

            context.Log.Message(@"Folder(s) monitored:
{0}", 
                String.Join(Environment.NewLine, 
                    folders.Select(x => String.Concat(" - ", x.ToString()))));

            foreach (MonitorConfiguration.MonitorFoldersConfiguration.Folder folder in folders)
            {
                string[] files =
                    Directory.EnumerateFiles(folder.Path, folder.SearchPattern ?? "*", folder.IncludeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                        .Where(folder.Criteria.IsSatisfiedBy)
                        .ToArray();

                if (files.Length > 0)
                {
                    context.Log.Message("{0} file(s) matched by '{1}'.", files.Length, folder);

                    var message = new StringBuilder();
                    message.AppendFormat("{0} file(s) matching criteria: '{1}'.", files.Length, folder.Criteria);
                    message.AppendLine();

                    const int limit = 10;

                    message.AppendLine(String.Join(Environment.NewLine, files
                        .Take(limit)
                        .Select(x => String.Format(" - {0}", x))));

                    if (files.Length > limit)
                        message.AppendLine("...");

                    workItem.Add(Time.UtcNow, this.Name(), message.ToString(), folder.Target);
                }
            }
        }

        public override string Description
        {
            get { return "Monitors configured folders."; }
        }
    }
}