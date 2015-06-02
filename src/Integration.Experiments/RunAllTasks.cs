using System.Linq;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class RunAllTasks : Task
    {
        private readonly ITaskFactory _factory;
        private readonly ITaskRunner _runner;

        public RunAllTasks(ITaskFactory factory, ITaskRunner runner)
        {
            _factory = factory;
            _runner = runner;
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            foreach (ITask task in _factory.GetAll().Except(new[] {this}))
                _runner.Execute(task);
        }

        public override string Description
        {
            get { return "Runs all tasks."; }
        }
    }
}