using System;
using System.Linq;

namespace Vertica.Integration.Model.Startup
{
    internal class ExecutionContext
    {
        public ExecutionContext(string taskName, ITask task, params string[] arguments)
        {
            if (String.IsNullOrWhiteSpace(taskName)) throw new ArgumentException(@"Value cannot be null or empty.", "taskName");
            if (task == null) throw new ArgumentNullException("task");

            TaskName = taskName;
            Task = task;
            arguments = arguments ?? new string[0];

            int indexOfAction = Array.FindLastIndex(arguments, x => x.StartsWith("-"));

            if (indexOfAction >= 0)
            {
                ActionName = arguments[indexOfAction].Substring(1);
                ActionArguments = arguments.Skip(indexOfAction + 1).ToArray();

                TaskArguments = arguments.Take(indexOfAction).ToArray();
            }
            else
            {
                ActionName = String.Empty;
                ActionArguments = new string[0];

                TaskArguments = arguments;
            }
        }

        public string TaskName { get; private set; }
        public string[] TaskArguments { get; private set; }

        public ITask Task { get; private set; }

        public string ActionName { get; private set; }
        public string[] ActionArguments { get; private set; }
    }
}