using System;
using System.Collections.Generic;
using System.IO;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model.Exceptions;
using Vertica.Integration.Model.Tasks;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Model
{
	public class TaskRunner : ITaskRunner
	{
	    private readonly ILogger _logger;
	    private readonly IConcurrentTaskExecution _concurrentTaskExecution;
	    private readonly IShutdown _shutdown;
	    private readonly TextWriter _outputter;

	    public TaskRunner(ILogger logger, IConcurrentTaskExecution concurrentTaskExecution, IShutdown shutdown, TextWriter outputter)
		{
	        _logger = logger;
		    _concurrentTaskExecution = concurrentTaskExecution;
	        _shutdown = shutdown;
		    _outputter = outputter;
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

			Action<string> outputter = message =>
			{
				message = $"[{Time.Now:HH:mm:ss}] {message}";

				_outputter.WriteLine(message);
				output.Add(message);
			};

            using (var taskLog = new TaskLog(task, _logger.LogEntry, new Output(outputter)))
            using (_concurrentTaskExecution.Handle(task, arguments, taskLog))
            {
                TWorkItem workItem;

                try
                {
                    var context = new TaskExecutionContext(new Log(taskLog.LogMessage, _logger), arguments, _shutdown.Token);
                    context.ThrowIfCancelled();

                    workItem = task.Start(context);
                }
                catch (Exception ex)
                {
                    ErrorLog errorLog = _logger.LogError(ex);
                    taskLog.ErrorLog = errorLog;

                    throw new TaskExecutionFailedException($"Starting Task '{taskLog.Name}' failed with message: '{ex.Message}'. ErrorID: {errorLog?.Id ?? "<null>"}.", ex);
                }

                try
                {
                    foreach (IStep<TWorkItem> step in task.Steps)
                    {
                        Execution continueWith = step.ContinueWith(workItem);

                        if (continueWith == Execution.StepOut)
                            break;

                        if (continueWith == Execution.StepOver)
                            continue;

                        using (StepLog stepLog = taskLog.LogStep(step))
                        {
                            try
                            {
                                var context = new TaskExecutionContext(new Log(stepLog.LogMessage, _logger), arguments, _shutdown.Token);
                                context.ThrowIfCancelled();

                                step.Execute(workItem, context);
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
                        var context = new TaskExecutionContext(new Log(taskLog.LogMessage, _logger), arguments, _shutdown.Token);
                        context.ThrowIfCancelled();

                        task.End(workItem, context);
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
                    workItem.DisposeIfDisposable();
                }
            }

			return new TaskExecutionResult(output.ToArray());
		}
	}
}