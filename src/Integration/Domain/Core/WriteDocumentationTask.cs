using System;
using System.IO;
using System.Text;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.Core
{
    public class WriteDocumentationTask : Task
    {
        private readonly ITaskService _taskService;
	    private readonly string _fileName;

	    public WriteDocumentationTask(ITaskService taskService, string fileName)
        {
	        _taskService = taskService;
	        _fileName = fileName;
        }

		public override void StartTask(Log log, params string[] arguments)
        {
            var sb = new StringBuilder();

		    foreach (ITask task in _taskService.GetAll())
            {
                sb.AppendLine(task.DisplayName);
                sb.AppendLine();
                sb.AppendLine(String.Format("\t- {0}", task.Description));

                foreach (IStep step in task.Steps)
                    sb.AppendLine(String.Format("\t\t- {0}", step.Description));

                sb.AppendLine();
                sb.AppendLine(String.Format("Schedule: {0}", task.Schedule));
                sb.AppendLine("-------------------------------------");
                sb.AppendLine();
            }

			File.WriteAllText(_fileName, sb.ToString());
        }

        public override string Description
        {
            get { return "Outputs all integration tasks and related steps."; }
        }

        public override string Schedule
        {
            get { return "Manually."; }
        }
    }
}