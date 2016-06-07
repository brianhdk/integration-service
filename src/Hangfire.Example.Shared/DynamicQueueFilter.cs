using System;
using System.Configuration;
using Hangfire.States;

namespace Hangfire.Example.Shared
{
	public class DynamicQueueFilter : IElectStateFilter
	{
		public void OnStateElection(ElectStateContext context)
		{
			var enqueuedState = context.CandidateState as EnqueuedState;

			if (enqueuedState != null)
			{
				enqueuedState.Queue = QueueName.Value;
			}
		}

		public static BackgroundJobServerOptions Apply(BackgroundJobServerOptions options)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));

			options.Queues = new[] { QueueName.Value };

			GlobalJobFilters.Filters.Add(new DynamicQueueFilter());

			return options;
		}

		private static readonly Lazy<string> QueueName = new Lazy<string>(() =>
		{
			string queueName = ConfigurationManager.AppSettings["Hangfire.QueueName"];

			return (queueName ?? Environment.MachineName).ToLowerInvariant().Replace("-", "_");
		});
	}
}