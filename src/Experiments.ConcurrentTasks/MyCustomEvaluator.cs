using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Tasks;
using Vertica.Integration.Model.Tasks.Evaluators;

namespace Experiments.ConcurrentTasks
{
    public class MyCustomEvaluator : IPreventConcurrentTaskExecutionRuntimeEvaluator
    {
        private readonly DisabledIfIntegrationDbIsDisabled _inner;

        public MyCustomEvaluator(DisabledIfIntegrationDbIsDisabled inner)
        {
            _inner = inner;
        }

        public bool Disabled(ITask currentTask, Arguments arguments)
        {
            return
                _inner.Disabled(currentTask, arguments) ||
                arguments.Contains("AllowConcurrentExecution");
        }
    }

    public class MyCustomLockName : IPreventConcurrentTaskExecutionCustomLockName
    {
        public string GetLockName(ITask currentTask, Arguments arguments)
        {
            return $"{currentTask.Name()}_{arguments["LockNamePostfix"]}";
        }
    }
}