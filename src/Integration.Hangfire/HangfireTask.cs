using System;
using Vertica.Integration.Model;

namespace Integration.Hangfire
{
	public class HangfireTask : Task
	{
		public override string Description => "Test Hangfire";

		public override void StartTask(ITaskExecutionContext context)
		{
			context.Log.Message("{0} - {1}", context.Arguments.ToString(), DateTime.Now);
		}
	}
}