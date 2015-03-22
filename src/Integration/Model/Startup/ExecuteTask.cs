using System;

namespace Vertica.Integration.Model.Startup
{
    internal class ExecuteTask : StartupAction
    {
        private readonly ITaskRunner _runner;

        public ExecuteTask(ITaskRunner runner)
        {
            _runner = runner;
        }

        protected override string ActionName
        {
            get { return String.Empty; }
        }

        protected override ArgumentValidator Validator
        {
            get { return null; }
        }

        protected override void DoExecute(ExecutionContext context)
        {
            _runner.Execute(context.Task, context.TaskArguments);
        }
    }
}