using System;
using System.Linq;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Model;
using Vertica.Integration.Portal;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			if (args == null) throw new ArgumentNullException("args");
			if (args.Length == 0) throw new ArgumentOutOfRangeException("args", @"No task name passed as argument");

			using (var context = ApplicationContext.Create(builder => builder
                //.UsePortal()
                //.AutoRegistredTasks(autoTasks => autoTasks
                //    .Skip<WriteDocumentationTask>()
                //    .Skip<MigrateTask>())
                .Nothing()))
			{
				ITaskService taskService = context.TaskService;

				taskService.Execute(args.First(), args.Skip(1).ToArray());
			}
		}
	}
}