using System;
using System.IO;
using System.Linq;
using System.Text;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Core
{
    public class WriteDocumentationTask : Task
    {
        private readonly ITaskFactory _taskFactory;

        public WriteDocumentationTask(ITaskFactory taskFactory)
        {
            _taskFactory = taskFactory;
        }

        public override void StartTask(ILog log, params string[] arguments)
        {
            var sb = new StringBuilder();

            Func<string, int, string> makeIndent = (msg, count) => 
                String.Concat(new string(' ', count), msg);

            foreach (ITask task in _taskFactory.GetAll())
            {
                sb.AppendLine(task.Name());
                sb.AppendLine(makeIndent(task.Description, 3));
                sb.AppendLine();

                foreach (IStep step in task.Steps)
                {
                    sb.AppendLine(makeIndent(step.Name(), 3));
                    sb.AppendLine(makeIndent(step.Description, 6));
                    sb.AppendLine();
                }
            }

            string text = sb.ToString();

            if (arguments != null && arguments.Contains("ToFile", StringComparer.OrdinalIgnoreCase))
            {
                var file = new FileInfo("Tasks-Documentation.txt");

                File.WriteAllText(file.Name, text);

                log.Message("File generated. Location: {0}", file.FullName);
            }
            else
            {
                log.Message(text);
            }
        }

        public override string Description
        {
            get
            {
                return @"Outputs all integration tasks and related steps. Use argument ""ToFile"" to generate a text-file with this documentation.";
            }
        }
    }
}