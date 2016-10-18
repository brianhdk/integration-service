using System;
using Vertica.Integration.Infrastructure.Database;

namespace Vertica.Integration.Model.Tasks.Evaluators
{
    public sealed class DisabledIfIntegrationDbIsDisabled : IPreventConcurrentTaskExecutionRuntimeEvaluator
    {
        private readonly IDatabaseConfiguration _configuration;

        public DisabledIfIntegrationDbIsDisabled(IDatabaseConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _configuration = configuration;
        }

        public bool Disabled(ITask currentTask, Arguments arguments)
        {
            return _configuration.IntegrationDbDisabled;
        }
    }
}