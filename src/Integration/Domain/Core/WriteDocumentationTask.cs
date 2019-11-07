using System;
using System.IO;
using System.Text;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Domain.Core
{
#pragma warning disable 618
    public class WriteDocumentationTask : Task
#pragma warning restore 618
    {
        private readonly ITaskFactory _taskFactory;
        private readonly IHostFactory _hostFactory;

        public WriteDocumentationTask(ITaskFactory taskFactory, IHostFactory hostFactory)
        {
            _taskFactory = taskFactory;
            _hostFactory = hostFactory;
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            Func<string, int, string> indent = (msg, count) =>
				string.Concat(new string(' ', count), msg);

            foreach (IHost host in _hostFactory.GetAll())
            {
                sb.AppendLine(host.Name());
                sb.AppendLine(indent(host.Description, 3));
                sb.AppendLine();
            }
            
            foreach (ITask task in _taskFactory.GetAll())
            {
                sb.AppendLine(task.Name());

				if (!string.IsNullOrWhiteSpace(task.Description))
					sb.AppendLine(indent(task.Description, 3));

                sb.AppendLine();

                foreach (IStep step in task.Steps)
                {
                    sb.AppendLine(indent(step.Name(), 3));
                    sb.AppendLine(indent(step.Description, 6));
                    sb.AppendLine();
                }
            }

            string text = sb.ToString();

            string path;
            if (context.Arguments.TryGetValue("ToFile", out path))
            {
                var file = new FileInfo(path ?? "Documentation.txt");

                File.WriteAllText(file.Name, text);

                context.Log.Message("File generated. Location: {0}", file.FullName);
            }
            else
            {
                context.Log.Message(text);
            }
        }

        public override string Description => @"Outputs all integration tasks and related steps. Use argument ""ToFile"" to generate a text-file with this documentation.";
    }
}