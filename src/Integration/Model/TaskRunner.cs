using System;
using System.Collections.Generic;
using System.IO;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model.Exceptions;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Model
{
	public class TaskRunner : ITaskRunner
	{
		private readonly ILogger _logger;
		private readonly TextWriter _outputter;

		public TaskRunner(ILogger logger, TextWriter outputter)
		{
			_logger = logger;
			_outputter = outputter;
		}

	    public TaskExecutionResult Execute(string taskName, ITask task, params string[] arguments)
	    {
            // latebound because we don't know the exact generic type at compile time
	        return ExecuteInternal(taskName, (dynamic) task, arguments);
	    }

	    private TaskExecutionResult ExecuteInternal<TWorkItem>(string taskName, ITask<TWorkItem> task, params string[] arguments)
		{
			if (task == null) throw new ArgumentNullException("task");

	        var output = new List<string>();

	        Action<string> outputter = message =>
	        {
	            message = String.Format("{0:HH:mm:ss}: {1}", Time.Now, message);

	            _outputter.WriteLine(message);
	            output.Add(message);
	        };

	        Action<Target, string> logWarning = (target, message) =>
	        {
	            outputter(message);
	            _logger.LogWarning(target, message);
	        };

            using (var taskLog = new TaskLog(taskName, _logger.LogEntry, new Output(outputter)))
			{
				TWorkItem workItem;

				try
				{
					workItem = task.Start(new Log(taskLog.LogMessage, logWarning), arguments);
				}
				catch (Exception ex)
				{
					ErrorLog errorLog = _logger.LogError(ex);
					taskLog.ErrorLog = errorLog;

					throw new TaskExecutionFailedException("Starting task failed.", ex);
				}

				foreach (IStep<TWorkItem> step in task.Steps)
				{
					Execution continueWith = step.ContinueWith(workItem);

					if (continueWith == Execution.StepOut)
						break;

					if (continueWith == Execution.StepOver)
						continue;

					using (var stepLog = taskLog.LogStep(GetStepName(step)))
					{
						try
						{
							step.Execute(workItem, new Log(stepLog.LogMessage, logWarning));
						}
						catch (Exception ex)
						{
							ErrorLog errorLog = _logger.LogError(ex);

							taskLog.ErrorLog = errorLog;
							stepLog.ErrorLog = errorLog;

							throw new TaskExecutionFailedException(String.Format("Step '{0}' failed.", stepLog.StepName), ex);
						}
					}
				}

				try
				{
					task.End(workItem, new Log(taskLog.LogMessage, logWarning), arguments);
				}
				catch (Exception ex)
				{
					ErrorLog errorLog = _logger.LogError(ex);
					taskLog.ErrorLog = errorLog;

					throw new TaskExecutionFailedException("Ending task failed.", ex);
				}
			}

            return new TaskExecutionResult(output.ToArray());
		}

		internal static string GetStepName(IStep step)
		{
			if (step == null) throw new ArgumentNullException("step");

			return step.GetType().Name;
		}
	}
}