using System;
using System.Linq;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Experiments.Custom_Database;
using Vertica.Integration.Experiments.Custom_Database.Migrations;
using Vertica.Integration.Infrastructure.Database.Dapper.Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Logging.Kibana;
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

		    var customDb = new CustomDb();

			using (var context = ApplicationContext.Create(builder => builder.UseKibana()))
			{
				ITaskService taskService = context.TaskService;

				taskService.Execute(args.First(), args.Skip(1).ToArray());
			}
		}
	}
}