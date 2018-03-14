using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;
using Vertica.Utilities.Extensions.EnumerableExt;

namespace Vertica.Integration.Domain.Monitoring
{
    public class ExportIntegrationErrorsStep : Step<MonitorWorkItem>
    {
        internal const string MessageGroupingPattern = @"ErrorID: .+$";

        private readonly Lazy<IDbFactory> _db;
        private readonly IIntegrationDatabaseConfiguration _configuration;
        private readonly ITaskFactory _taskFactory;

        public ExportIntegrationErrorsStep(Lazy<IDbFactory> db, IIntegrationDatabaseConfiguration configuration, ITaskFactory taskFactory)
        {
            _db = db;
            _configuration = configuration;
            _taskFactory = taskFactory;
        }

        public override Execution ContinueWith(ITaskExecutionContext<MonitorWorkItem> context)
        {
            if (_configuration.Disabled)
                return Execution.StepOver;

            return Execution.Execute;
        }

        public override void Execute(ITaskExecutionContext<MonitorWorkItem> context)
        {
            context.WorkItem.AddMessageGroupingPatterns(MessageGroupingPattern);

            using (IDbSession session = _db.Value.OpenSession())
            {
                ErrorEntry[] errors = session.Query<ErrorEntry>($@"
SELECT
	ErrorLog.Id AS ErrorId,
	ErrorLog.[Message] AS ErrorMessage,
	ErrorLog.[TimeStamp] AS [DateTime],
	ErrorLog.Severity,
	ErrorLog.[Target],
	TaskLog.TaskName,
	TaskLog.StepName
FROM
	[{_configuration.TableName(IntegrationDbTable.ErrorLog)}] AS ErrorLog
	OUTER APPLY (
		SELECT TOP 1 TaskLog.TaskName, TaskLog.StepName
		FROM [{_configuration.TableName(IntegrationDbTable.TaskLog)}] AS TaskLog
		WHERE (TaskLog.ErrorLog_Id = ErrorLog.Id)
		ORDER BY ID DESC
	) AS TaskLog
WHERE (
	ErrorLog.[TimeStamp] BETWEEN @LowerBound AND @UpperBound
)
ORDER BY ErrorLog.[TimeStamp] DESC",
                    new
                    {
                        context.WorkItem.CheckRange.LowerBound,
                        context.WorkItem.CheckRange.UpperBound
                    }).ToArray();

                if (errors.Length > 0)
                {
                    context.Log.Message("{0} entries within time-period {1}.", errors.Length, context.WorkItem.CheckRange);

                    var tasksByName = new Dictionary<string, ITask>(StringComparer.OrdinalIgnoreCase);

                    foreach (ErrorEntry error in errors)
                    {
                        var taskName = error.SafeTaskName();

                        ITask task = null;

                        if (!string.IsNullOrWhiteSpace(taskName) && !tasksByName.TryGetValue(taskName, out task))
                        {
                            try
                            {
                                task = _taskFactory.Get(taskName);
                                tasksByName.Add(taskName, task);
                            }
                            catch (TaskNotFoundException)
                            {
                            }
                        }

                        if (task != null)
                        {
                            error.TaskDescription = task.Description;

                            IStep step =
                                task.Steps.EmptyIfNull()
                                    .SingleOrDefault(x =>
										string.Equals(x.Name(), error.StepName,
                                            StringComparison.OrdinalIgnoreCase));

                            if (step != null)
                                error.StepDescription = step.Description;
                        }

                        context.WorkItem.Add(error.DateTime, "Integration Service", error.CombineMessage(), error.Target);
                    }
                }
            }
        }

        public override string Description => $"Exports errors from table {_configuration.TableName(IntegrationDbTable.ErrorLog)}.";

        public class ErrorEntry
        {
            public int ErrorId { get; set; }
            public string ErrorMessage { get; set; }
            public DateTimeOffset DateTime { get; set; }
            public Severity Severity { get; set; }
            public Target Target { get; set; }
            public string TaskName { get; set; }
            public string StepName { get; set; }
            public string TaskDescription { get; set; }
            public string StepDescription { get; set; }

            public string SafeTaskName()
            {
                return TaskName ?? string.Empty;
            }

            public string CombineMessage()
            {
                var sb = new StringBuilder();

                sb.AppendFormat("{0}:", Severity);
                sb.AppendLine();

                var taskName = SafeTaskName();

                if (!string.IsNullOrWhiteSpace(taskName))
                {
                    sb.AppendLine();
                    sb.AppendFormat("Task '{0}'", taskName);

                    if (!string.IsNullOrWhiteSpace(TaskDescription))
                    {
                        sb.AppendFormat(": {0}", TaskDescription);
                        sb.AppendLine();
                    }

                    if (!string.IsNullOrWhiteSpace(StepName))
                    {
                        sb.AppendFormat("Step '{0}'", StepName);

                        if (!string.IsNullOrWhiteSpace(StepDescription))
                            sb.AppendFormat(": {0}", StepDescription);
                    }

                    sb.AppendLine();
                    sb.AppendLine();
                }

                sb.Append(ErrorMessage);

                sb.AppendLine();
                sb.AppendLine();
                sb.AppendFormat("ErrorID: {0}", ErrorId);

                return sb.ToString();
            }
        }
    }
}