using System;
using System.Collections.Generic;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model.Exceptions;
using Vertica.Integration.Model.Tasks;
using Vertica.Utilities;

namespace Vertica.Integration.Model
{
	public class TaskRunner : ITaskRunner
	{
	    private readonly ILogger _logger;
	    private readonly IConcurrentTaskExecution _concurrentTaskExecution;
	    private readonly IShutdown _shutdown;
	    private readonly IConsoleWriter _console;

	    public TaskRunner(ILogger logger, IConcurrentTaskExecution concurrentTaskExecution, IShutdown shutdown, IConsoleWriter console)
		{
	        _logger = logger;
		    _concurrentTaskExecution = concurrentTaskExecution;
	        _shutdown = shutdown;
		    _console = console;
		}

		public TaskExecutionResult Execute(ITask task, Arguments arguments = null)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));

			// latebound because we don't know the exact generic type at compile time
			return ExecuteInternal((dynamic) task, arguments ?? Arguments.Empty);
		}

		private TaskExecutionResult ExecuteInternal<TWorkItem>(ITask<TWorkItem> task, Arguments arguments)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));
			if (arguments == null) throw new ArgumentNullException(nameof(arguments));

			var output = new List<string>();

		    void Outputter(string message)
		    {
		        message = $"[{Time.Now:HH:mm:ss}] {message}";

		        _console.WriteLine(message);
		        output.Add(message);
		    }

		    using (var taskLog = new TaskLog(task, _logger.LogEntry, new Output(Outputter)))
            using (ConcurrentTaskExecutionResult taskExecution = HandleConcurrentTaskExecution(task, arguments, taskLog))
            {
                if (taskExecution.StopTask)
                    return new TaskExecutionResult(output.ToArray());

                Action<string>[] logMessage = { x => { } };

                var log = new Log(message => logMessage[0]?.Invoke(message), _logger);
                var context = new TaskExecutionContext<TWorkItem>(taskLog, log, arguments, _shutdown.Token);

                if (task.IsDisabled(context))
                {
                    Outputter($"Task '{task.Name()}' is disabled.");

                    return new TaskExecutionResult(output.ToArray());
                }

                try
                {
                    logMessage[0] = taskLog.LogMessage;
                    context.ThrowIfCancelled();

                    context.WorkItem = task.Start(context);
                }
                catch (Exception ex)
                {
                    ErrorLog errorLog = _logger.LogError(ex);
                    taskLog.ErrorLog = errorLog;

                    throw new TaskExecutionFailedException($"Starting Task '{taskLog.Name}' failed with message: '{ex.DestructMessage()}'. ErrorID: {errorLog?.Id ?? "<null>"}.", ex);
                }

                try
                {
                    foreach (IStep<TWorkItem> step in task.Steps)
                    {
                        using (StepLog stepLog = taskLog.LogStep(step))
                        {
                            try
                            {
                                logMessage[0] = stepLog.LogMessage;
                                context.ThrowIfCancelled();

                                Execution continueWith = step.ContinueWith(context);

                                if (continueWith == Execution.StepOut)
                                    break;

                                if (continueWith == Execution.StepOver)
                                    continue;

                                context.ThrowIfCancelled();

                                step.Execute(context);
                            }
                            catch (Exception ex)
                            {
                                ErrorLog errorLog = _logger.LogError(ex);

                                taskLog.ErrorLog = errorLog;
                                stepLog.ErrorLog = errorLog;

                                throw new TaskExecutionFailedException($"Step '{stepLog.Name}' on Task '{taskLog.Name}' failed with message: '{ex.Message}'. ErrorID: {errorLog?.Id ?? "<null>"}.", ex);
                            }
                        }
                    }

                    try
                    {
                        logMessage[0] = taskLog.LogMessage;
                        context.ThrowIfCancelled();

                        task.End(context);
                    }
                    catch (Exception ex)
                    {
                        ErrorLog errorLog = _logger.LogError(ex);
                        taskLog.ErrorLog = errorLog;

                        throw new TaskExecutionFailedException($"Ending Task '{taskLog.Name}' failed with message: '{ex.Message}'. ErrorID: {errorLog?.Id ?? "<null>"}.", ex);
                    }
                }
                finally
                {
                    context.WorkItem.DisposeIfDisposable();
                }
            }

			return new TaskExecutionResult(output.ToArray());
		}

	    private ConcurrentTaskExecutionResult HandleConcurrentTaskExecution(ITask task, Arguments arguments, TaskLog taskLog)
	    {
	        try
	        {
	            ConcurrentTaskExecutionResult result = _concurrentTaskExecution.Handle(task, arguments, taskLog);

                if (result == null)
                    throw new InvalidOperationException($"{_concurrentTaskExecution.GetType().FullName} returned <null> which was not expected.");

	            return result;
	        }
	        catch (Exception ex)
	        {
                ErrorLog errorLog = _logger.LogError(ex);
                taskLog.ErrorLog = errorLog;

                throw new TaskExecutionFailedException($"Starting Task '{taskLog.Name}' failed with message: '{ex.DestructMessage()}'. ErrorID: {errorLog?.Id ?? "<null>"}.", ex);
            }
	    }
	}
}