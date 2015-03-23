using System;
using System.Linq;
using Vertica.Integration;
using Vertica.Integration.Model;

namespace $rootnamespace$
{
	public static class IntegrationStartup
	{
		public static void Run(string[] args, Action<ApplicationConfiguration> builder = null)
		{
			if (args == null || args.Length == 0) throw new ArgumentException("Expected at least a TaskName to be passed as the first argument.", "args");

			using (var context = ApplicationContext.Create(cfg => cfg.Change(builder)
            ))
			{
				ITaskService taskService = context.TaskService;

				taskService.Execute(args.First(), args.Skip(1).ToArray());
			}
		}
	}
}