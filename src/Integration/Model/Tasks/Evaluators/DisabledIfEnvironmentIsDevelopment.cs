using System;

namespace Vertica.Integration.Model.Tasks.Evaluators
{
    public sealed class DisabledIfEnvironmentIsDevelopment : IPreventConcurrentTaskExecutionRuntimeEvaluator
    {
        private readonly IRuntimeSettings _settings;

        public DisabledIfEnvironmentIsDevelopment(IRuntimeSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _settings = settings;
        }

        public bool Disabled(ITask currentTask, Arguments arguments)
        {
            return _settings.Environment == ApplicationEnvironment.Development;
        }
    }
}