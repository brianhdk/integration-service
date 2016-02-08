using System;
using Vertica.Integration.Model;

namespace Integration.Experiments.Hangfire
{
	public class HangfireTask : Task
	{
		public override string Description
		{
			get { return "Test Hangfire"; }
		}

		public override void StartTask(ITaskExecutionContext context)
		{
			context.Log.Message("{0} - {1}", context.Arguments.ToString(), DateTime.Now);
		}
	}
}