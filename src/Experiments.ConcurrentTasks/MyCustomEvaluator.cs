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

        public bool Disabled(ITask currentTask)
        {
            return _inner.Disabled(currentTask);
        }
    }
}