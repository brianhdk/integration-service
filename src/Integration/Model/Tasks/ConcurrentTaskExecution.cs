using System;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Threading.DistributedMutex;
using Vertica.Integration.Model.Exceptions;
using Vertica.Utilities_v4.Extensions.AttributeExt;

namespace Vertica.Integration.Model.Tasks
{
    public class ConcurrentTaskExecution : IConcurrentTaskExecution
    {
        private readonly IDistributedMutex _distributedMutex;
        private readonly ILogger _logger;

        private readonly DistributedMutexConfiguration _defaultConfiguration;
        private readonly bool _enabledOnAllTasks;

        public ConcurrentTaskExecution(IDistributedMutex distributedMutex, ILogger logger, IRuntimeSettings settings)
        {
            _distributedMutex = distributedMutex;
            _logger = logger;

            _defaultConfiguration = DefaultConfiguration(settings, out _enabledOnAllTasks);
        }

        public IDisposable Handle(ITask task, TaskLog log)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (log == null) throw new ArgumentNullException(nameof(log));

            var preventConcurrent = task.GetAttribute<PreventConcurrentExecutionAttribute>();

            if (preventConcurrent == null)
            {
                if (task.HasAttribute<AllowConcurrentExecutionAttribute>())
                    return null;

                if (!_enabledOnAllTasks)
                    return null;
            }

            try
            {
                var context = new DistributedMutexContext(task.Name(), preventConcurrent?.Configuration ?? _defaultConfiguration, log.LogMessage);

                return _distributedMutex.Enter(context);
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = _logger.LogError(ex);
                log.ErrorLog = errorLog;

                throw new TaskExecutionFailedException("Unable to acquire lock.", ex);
            }
        }

        private static DistributedMutexConfiguration DefaultConfiguration(IRuntimeSettings settings, out bool enabledOnAllTasks)
        {
            TimeSpan defaultWaitTime;
            if (!TimeSpan.TryParse(settings[$"{nameof(ConcurrentTaskExecution)}.DefaultWaitTime"], out defaultWaitTime))
                defaultWaitTime = TimeSpan.FromSeconds(30);

            bool.TryParse(settings[$"{nameof(ConcurrentTaskExecution)}.EnabledOnAllTasks"], out enabledOnAllTasks);

            return new DistributedMutexConfiguration(defaultWaitTime);
        }
    }
}