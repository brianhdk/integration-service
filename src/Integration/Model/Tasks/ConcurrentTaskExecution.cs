using System;
using System.Reflection;
using Castle.MicroKernel;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Threading.DistributedMutex;
using Vertica.Integration.Model.Exceptions;

namespace Vertica.Integration.Model.Tasks
{
    public class ConcurrentTaskExecution : IConcurrentTaskExecution
    {
        private readonly IDistributedMutex _distributedMutex;
        private readonly IKernel _kernel;

        private readonly DistributedMutexConfiguration _defaultConfiguration;
        private readonly bool _preventConcurrentTaskExecutionOnAllTasks;

        public ConcurrentTaskExecution(IDistributedMutex distributedMutex, IKernel kernel, IRuntimeSettings settings)
        {
            _distributedMutex = distributedMutex;
            _kernel = kernel;

            _defaultConfiguration = DefaultConfiguration(settings, out _preventConcurrentTaskExecutionOnAllTasks);
        }

        public ConcurrentTaskExecutionResult Handle(ITask task, Arguments arguments, TaskLog log)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            if (log == null) throw new ArgumentNullException(nameof(log));

            Type taskType = task.GetType();

            var preventConcurrent = taskType.GetCustomAttribute<PreventConcurrentTaskExecutionAttribute>();

            if (preventConcurrent == null)
            {
                var allowConcurrentTaskExecutionAttribute = taskType.GetCustomAttribute<AllowConcurrentTaskExecutionAttribute>();

                if (allowConcurrentTaskExecutionAttribute != null)
                    return ConcurrentTaskExecutionResult.Continue();

                if (!_preventConcurrentTaskExecutionOnAllTasks)
                    return ConcurrentTaskExecutionResult.Continue();
            }
            else
            {
                if (IsDisabled(task, arguments, preventConcurrent))
                    return ConcurrentTaskExecutionResult.Continue();
            }

            string lockName = GetLockName(task, arguments, preventConcurrent);
            string lockDescription = GetLockDescription(task, log, arguments, preventConcurrent);
            IPreventConcurrentTaskExecutionExceptionHandler exceptionHandler = GetExceptionHandler(task, preventConcurrent);

            try
            {
                DistributedMutexConfiguration configuration = preventConcurrent?.Configuration ?? _defaultConfiguration;

                var context = new DistributedMutexContext(lockName, configuration, log.LogMessage, lockDescription);

                IDisposable lockAcquired = _distributedMutex.Enter(context);

                return ConcurrentTaskExecutionResult.Continue(lockAcquired);
            }
            catch (Exception ex)
            {
                if (exceptionHandler != null)
                {
                    ex = exceptionHandler.OnException(task, log, arguments, ex);

                    if (ex == null)
                        return ConcurrentTaskExecutionResult.Stop();
                }

                throw new TaskExecutionLockNotAcquiredException($"Unable to acquire lock '{lockName}'.", ex);
            }
        }

        private bool IsDisabled(ITask task, Arguments arguments, PreventConcurrentTaskExecutionAttribute preventConcurrent)
        {
            Type runtimeEvaluatorType = preventConcurrent.RuntimeEvaluator;

            if (runtimeEvaluatorType != null)
            {
                if (!(_kernel.Resolve(runtimeEvaluatorType) is IPreventConcurrentTaskExecutionRuntimeEvaluator runtimeEvaluator)
                )
                    throw new InvalidOperationException($@"Unable to resolve evaluator type '{runtimeEvaluatorType.FullName}' which has been specified on task '{task.GetType().FullName}'. 

Either the type does not implement '{nameof(IPreventConcurrentTaskExecutionRuntimeEvaluator)}'-interface or you forgot to register the type as a custom evaluator. 

You can register custom evaluators when setting up Integration Service in the ApplicationContext.Create(...)-method:

using (IApplicationContext context = ApplicationContext.Create(application => application
    .Tasks(tasks => tasks
        .ConcurrentTaskExecution(concurrentTaskExecution => concurrentTaskExecution
            .AddFromAssemblyOfThis<Program>()
            .AddEvaluator<MyCustomEvaluator>()))");

                if (runtimeEvaluator.Disabled(task, arguments))
                    return true;
            }

            return false;
        }

        private string GetLockName(ITask task, Arguments arguments, PreventConcurrentTaskExecutionAttribute preventConcurrent)
        {
            Type customLockNameType = preventConcurrent?.CustomLockName;

            if (customLockNameType != null)
            {
                if (!(_kernel.Resolve(customLockNameType) is IPreventConcurrentTaskExecutionCustomLockName customLockName))
                    throw new InvalidOperationException(
                        $@"Unable to resolve custom lock name type '{customLockNameType.FullName}' which has been specified on task '{task.GetType().FullName}'. 

Either the type does not implement '{nameof(IPreventConcurrentTaskExecutionCustomLockName)}'-interface or you forgot to register the type as a custom lock name. 

You can register custom evaluators when setting up Integration Service in the ApplicationContext.Create(...)-method:

using (IApplicationContext context = ApplicationContext.Create(application => application
    .Tasks(tasks => tasks
        .ConcurrentTaskExecution(concurrentTaskExecution => concurrentTaskExecution
            .AddFromAssemblyOfThis<Program>()
            .AddCustomLockName<MyCustomLockName>()))");

                string lockName = customLockName.GetLockName(task, arguments);

                if (!string.IsNullOrWhiteSpace(lockName))
                    return lockName;
            }

            return task.Name();
        }

        private string GetLockDescription(ITask task, TaskLog log, Arguments arguments, PreventConcurrentTaskExecutionAttribute preventConcurrent)
        {
            string description = $"Acquired by '{task.Name()}'. TaskLogId: {log.Id ?? "<n/a>"}.";

            Type customLockDescriptionType = preventConcurrent?.CustomLockDescription;

            if (customLockDescriptionType != null)
            {
                if (!(_kernel.Resolve(customLockDescriptionType) is IPreventConcurrentTaskExecutionCustomLockDescription customLockDescription))
                    throw new InvalidOperationException(
                        $@"Unable to resolve custom lock description type '{customLockDescriptionType.FullName}' which has been specified on task '{task.GetType().FullName}'. 

Either the type does not implement '{nameof(IPreventConcurrentTaskExecutionCustomLockDescription)}'-interface or you forgot to register the type as a custom lock description. 

You can register custom lock descriptions when setting up Integration Service in the ApplicationContext.Create(...)-method:

using (IApplicationContext context = ApplicationContext.Create(application => application
    .Tasks(tasks => tasks
        .ConcurrentTaskExecution(concurrentTaskExecution => concurrentTaskExecution
            .AddFromAssemblyOfThis<Program>()
            .AddCustomLockDescription<MyCustomLockDescription>()))");

                description = customLockDescription.GetLockDescription(task, arguments, description);
            }

            return description;
        }

        private IPreventConcurrentTaskExecutionExceptionHandler GetExceptionHandler(ITask task, PreventConcurrentTaskExecutionAttribute preventConcurrent)
        {
            Type exceptionHandlerType = preventConcurrent?.ExceptionHandler;

            if (exceptionHandlerType != null)
            {
                if (!(_kernel.Resolve(exceptionHandlerType) is IPreventConcurrentTaskExecutionExceptionHandler exceptionHandler))
                    throw new InvalidOperationException(
                        $@"Unable to resolve exception handler type '{exceptionHandlerType.FullName}' which has been specified on task '{task.GetType().FullName}'. 

Either the type does not implement '{nameof(IPreventConcurrentTaskExecutionExceptionHandler)}'-interface or you forgot to register the type as an exception handler. 

You can register exception handlers when setting up Integration Service in the ApplicationContext.Create(...)-method:

using (IApplicationContext context = ApplicationContext.Create(application => application
    .Tasks(tasks => tasks
        .ConcurrentTaskExecution(concurrentTaskExecution => concurrentTaskExecution
            .AddFromAssemblyOfThis<Program>()
            .AddExceptionHandler<MyExceptionHandler>()))");

                return exceptionHandler;
            }

            return null;
        }

        private static DistributedMutexConfiguration DefaultConfiguration(IRuntimeSettings settings, out bool preventConcurrentTaskExecutionOnAllTasks)
        {
            if (!TimeSpan.TryParse(settings[$"{nameof(ConcurrentTaskExecution)}.DefaultWaitTime"], out TimeSpan defaultWaitTime))
                defaultWaitTime = TimeSpan.FromSeconds(30);

            bool.TryParse(settings[$"{nameof(ConcurrentTaskExecution)}.PreventConcurrentTaskExecutionOnAllTasks"], out preventConcurrentTaskExecutionOnAllTasks);

            return new DistributedMutexConfiguration(defaultWaitTime);
        }
    }
}