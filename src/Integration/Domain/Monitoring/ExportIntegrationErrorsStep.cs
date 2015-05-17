using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Domain.Monitoring
{
    public class ExportIntegrationErrorsStep : Step<MonitorWorkItem>
    {
        private readonly IDbFactory _db;
        private readonly ITaskFactory _taskFactory;

        public ExportIntegrationErrorsStep(IDbFactory db, ITaskFactory taskFactory)
        {
            _db = db;
            _taskFactory = taskFactory;
        }

        public override string Description
        {
            get { return "Exports errors from integration error log."; }
        }

        public override void Execute(MonitorWorkItem workItem, ILog log)
        {
            using (var session = _db.OpenSession())
            {
                ErrorEntry[] errors = session.Query<ErrorEntry>(@"
SELECT
	ErrorLog.Id AS ErrorId,
	ErrorLog.[Message] AS ErrorMessage,
	ErrorLog.[TimeStamp] AS [DateTime],
	ErrorLog.Severity,
	ErrorLog.[Target],
	TaskLog.TaskName,
	TaskLog.StepName
FROM
	ErrorLog
	OUTER APPLY (
		SELECT TOP 1 TaskLog.TaskName, TaskLog.StepName
		FROM TaskLog
		WHERE (TaskLog.ErrorLog_Id = ErrorLog.Id)
		ORDER BY ID DESC
	) AS TaskLog
WHERE (
	ErrorLog.[TimeStamp] BETWEEN @LowerBound AND @UpperBound
)
ORDER BY ErrorLog.Id DESC",
                    new
                    {
                        workItem.CheckRange.LowerBound,
                        workItem.CheckRange.UpperBound
                    }).ToArray();

                if (errors.Length > 0)
                {
                    log.Message("{0} errors/warnings within time-period {1}.", errors.Length, workItem.CheckRange);

                    var tasksByName = new Dictionary<string, ITask>(StringComparer.OrdinalIgnoreCase);

                    foreach (var error in errors)
                    {
                        var taskName = error.SafeTaskName();

                        ITask task = null;

                        if (!String.IsNullOrWhiteSpace(taskName) && !tasksByName.TryGetValue(taskName, out task))
                        {
                            try
                            {
                                task = _taskFactory.GetByName(taskName);
                                tasksByName.Add(taskName, task);
                            }
                            catch (TaskNotFoundException)
                            {
                            }
                        }

                        if (task != null)
                        {
                            error.TaskDescription = task.Description;

                            var step =
                                task.Steps.EmptyIfNull()
                                    .SingleOrDefault(x =>
                                        String.Equals(TaskRunner.GetStepName(x), error.StepName,
                                            StringComparison.OrdinalIgnoreCase));

                            if (step != null)
                                error.StepDescription = step.Description;
                        }

                        workItem.Add(
                            error.DateTime, 
                            "Integration Service", 
                            error.CombineMessage(), 
                            error.Target);
                    }
                }
            }
        }

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
                return TaskName ?? String.Empty;
            }

            public string CombineMessage()
            {
                var sb = new StringBuilder();

                sb.AppendFormat("{0}:", Severity);
                sb.AppendLine();

                var taskName = SafeTaskName();

                if (!String.IsNullOrWhiteSpace(taskName))
                {
                    sb.AppendLine();
                    sb.AppendFormat("Task '{0}'", taskName);

                    if (!String.IsNullOrWhiteSpace(TaskDescription))
                    {
                        sb.AppendFormat(": {0}", TaskDescription);
                        sb.AppendLine();
                    }

                    if (!String.IsNullOrWhiteSpace(StepName))
                    {
                        sb.AppendFormat("Step '{0}'", StepName);

                        if (!String.IsNullOrWhiteSpace(StepDescription))
                            sb.AppendFormat(": {0}", StepDescription);
                    }

                    sb.AppendLine();
                    sb.AppendLine();
                }

                sb.Append(ErrorMessage);

                sb.AppendLine();
                sb.AppendLine();
                sb.AppendFormat("ErrorID {0}:", ErrorId);

                return sb.ToString();
            }
        }
    }
}