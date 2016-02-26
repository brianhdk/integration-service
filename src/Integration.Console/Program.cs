using System;
using System.IO;
using Vertica.Integration.Experiments;
using Vertica.Integration.Model;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (IApplicationContext context = ApplicationContext.Create(application => application
				.NoDatabase()
				.Tasks(tasks => tasks.Task<WindowsScheduledTask>())
                .Void()))
			{
                context.Execute(args);
			}
		}
	}

	public class WindowsScheduledTask : Task
	{
		public override string Description { get; }
		public override void StartTask(ITaskExecutionContext context)
		{
			File.WriteAllText($@"c:\tmp\{Guid.NewGuid().ToString("D")}.txt", "Some file.");
		}
	}
}