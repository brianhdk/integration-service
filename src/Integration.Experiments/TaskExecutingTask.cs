using Vertica.Integration.Domain.Core;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class TaskExecutingTask : Task
    {
        private readonly ITaskRunner _runner;
        private readonly ITaskFactory _factory;

        public TaskExecutingTask(ITaskRunner runner, ITaskFactory factory)
        {
            _runner = runner;
            _factory = factory;
        }

        public override string Description
        {
            get { return "TBD"; }
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            context.Log.Message("This task is executing that task...");

            ITask task = _factory.Get<WriteDocumentationTask>();

            TaskExecutionResult result = _runner.Execute(task);

            context.Log.Message("That task resulted in {0} messages.", result.Output.Length);
        }
    }
}