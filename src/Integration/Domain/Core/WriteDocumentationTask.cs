using System;
using System.IO;
using System.Text;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Core
{
    public class WriteDocumentationTask : Task
    {
        private readonly ITaskService _taskService;

	    public WriteDocumentationTask(ITaskService taskService)
        {
	        _taskService = taskService;
        }

		public override void StartTask(Log log, params string[] arguments)
        {
            var sb = new StringBuilder();

		    foreach (ITask task in _taskService.GetAll())
            {
                sb.AppendLine(task.Name());
                sb.AppendLine();
                sb.AppendLine(String.Format("\t- {0}", task.Description));

                foreach (IStep step in task.Steps)
                    sb.AppendLine(String.Format("\t\t- {0}", step.Description));

                sb.AppendLine();
                sb.AppendLine("-------------------------------------");
                sb.AppendLine();
            }

			File.WriteAllText("Tasks-Documentation.txt", sb.ToString());
        }

        public override string Description
        {
            get { return "Outputs all integration tasks and related steps."; }
        }
    }
}