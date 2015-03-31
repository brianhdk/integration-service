using System;
using System.Linq;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;

namespace Vertica.Integration.Startup
{
    internal class ExecutionContext
    {
        public ExecutionContext(ITask task, params string[] arguments)
        {
            if (task == null) throw new ArgumentNullException("task");

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

        public ITask Task { get; private set; }
        public string TaskName { get { return Task.Name(); } }
        public string[] TaskArguments { get; private set; }

        public string ActionName { get; private set; }
        public string[] ActionArguments { get; private set; }
    }
}