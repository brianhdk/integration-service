using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Vertica.Integration.Infrastructure.Database.NHibernate;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Domain.Monitoring
{
    public class ExportIntegrationErrorsStep : Step<MonitorWorkItem>
	{
		private readonly Lazy<ISessionFactoryProvider> _sessionFactory;
		private readonly ITaskService _taskService;

		public ExportIntegrationErrorsStep(Lazy<ISessionFactoryProvider> sessionFactory, ITaskService taskService)
		{
			_sessionFactory = sessionFactory;
			_taskService = taskService;
		}

        public override void Execute(MonitorWorkItem workItem, Log log)
		{
			using (IStatelessSession session = _sessionFactory.Value.SessionFactory.OpenStatelessSession())
			{
				ErrorLog errorLogAlias = null;
				TaskLog taskLogAlias = null;
				StepLog stepLogAlias = null;
				ErrorEntry errorEntryAlias = null;

				IList<ErrorEntry> errors =
					session.QueryOver(() => errorLogAlias)
							.Where(x => x.TimeStamp.IsBetween(workItem.CheckRange.LowerBound).And(workItem.CheckRange.UpperBound))
							.SelectList(list => list
								.Select(() => errorLogAlias.Id).WithAlias(() => errorEntryAlias.ErrorId)
								.Select(() => errorLogAlias.Message).WithAlias(() => errorEntryAlias.ErrorMessage)
								.Select(() => errorLogAlias.TimeStamp).WithAlias(() => errorEntryAlias.DateTime)
                                .Select(() => errorLogAlias.Severity).WithAlias(() => errorEntryAlias.Severity)
                                .Select(() => errorLogAlias.Target).WithAlias(() => errorEntryAlias.Target)
								.SelectSubQuery(
									QueryOver.Of(() => taskLogAlias)
										.Where(taskLog => taskLog.ErrorLog.Id == errorLogAlias.Id)
										.Select(Projections.Property(() => taskLogAlias.TaskName))
								).WithAlias(() => errorEntryAlias.TaskNameFromTask)
								.SelectSubQuery(
									QueryOver.Of(() => stepLogAlias)
										.Where(stepLog => stepLog.ErrorLog.Id == errorLogAlias.Id)
										.Select(Projections.Property(() => stepLogAlias.TaskName))
								).WithAlias(() => errorEntryAlias.TaskNameFromStep)
								.SelectSubQuery(
									QueryOver.Of(() => stepLogAlias)
										.Where(stepLog => stepLog.ErrorLog.Id == errorLogAlias.Id)
										.Select(Projections.Property(() => stepLogAlias.StepName))
								).WithAlias(() => errorEntryAlias.StepName))
							.OrderBy(() => errorLogAlias.Id).Desc
							.TransformUsing(Transformers.AliasToBean<ErrorEntry>())
							.List<ErrorEntry>();

				if (errors.Count > 0)
				{
					log.Message("{0} errors/warnings within time-period {1}.", errors.Count, workItem.CheckRange);

				    var tasksByName = new Dictionary<string, ITask>(StringComparer.OrdinalIgnoreCase);

					foreach (ErrorEntry error in errors)
					{
					    string taskName = error.SafeTaskName();

					    ITask task = null;

					    if (!String.IsNullOrWhiteSpace(taskName) && !tasksByName.TryGetValue(taskName, out task))
					    {
					        try
					        {
					            task = _taskService.GetByName(taskName);
					            tasksByName.Add(taskName, task);
					        }
					        catch (TaskNotFoundException)
					        {
					        }
					    }

						if (task != null)
						{
							error.TaskDescription = task.Description;
							error.TaskSchedule = task.Schedule;

							IStep step =
								task.Steps.EmptyIfNull()
									.SingleOrDefault(x =>
										String.Equals(TaskRunner.GetStepName(x), error.StepName, StringComparison.OrdinalIgnoreCase));

							if (step != null)
								error.StepDescription = step.Description;
						}

						workItem.Add(error.DateTime, "Integration Service", error.CombineMessage(), error.Target);
					}
				}
			}
		}

        public override string Description { get { return "Exports errors from integration error log."; } }

		public class ErrorEntry
		{
			public int ErrorId { get; set; }
			public string ErrorMessage { get; set; }
			public DateTimeOffset DateTime { get; set; }
            public Severity Severity { get; set; }
            public Target Target { get; set; }

			public string TaskNameFromTask { get; set; }
			public string TaskNameFromStep { get; set; }
			public string StepName { get; set; }

			public string SafeTaskName()
			{
				return TaskNameFromTask ?? TaskNameFromStep ?? String.Empty;
			}

			public string TaskDescription { get; set; }
			public string TaskSchedule { get; set; }
			public string StepDescription { get; set; }

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
						sb.AppendFormat(": {0}, {1}", TaskDescription, TaskSchedule);
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