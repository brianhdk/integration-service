using System;
using System.Linq;
using Vertica.Integration.Model;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			if (args == null) throw new ArgumentNullException("args");
			if (args.Length == 0) throw new ArgumentOutOfRangeException("args", @"No task name passed as argument");

            // Make fluent configuration object to ApplicationContext.Create
                // name of database (if anything but default name (IntegrationDb)
                // custom installers of castle.windsor
                // custom xxx
                // which assemblies to scan for controllers

            //var configuration = new ApplicationConfiguration();

			using (var context = ApplicationContext.Create())
			{
				ITaskService taskService = context.TaskService;

				taskService.Execute(args.First(), args.Skip(1).ToArray());
			}
		}
	}
}