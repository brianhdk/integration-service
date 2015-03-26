using System;
using System.Linq;
using Vertica.Integration.Azure;
using Vertica.Integration.Experiments.Azure;
using Vertica.Integration.Experiments.Custom_Database;
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
                .UsePortal()
                .UseAzure(azure => azure.ReplaceArchiverWithBlobStorage("AzureBlobStorage.Archiver"))
                .AddDatabaseForDapperProvider(new CustomDb())
                .AutoRegistredTasks(auto => auto.Scan(typeof(AzureArchiverTesterTask).Assembly))))
			{
				ITaskService taskService = context.TaskService;

				taskService.Execute(args.First(), args.Skip(1).ToArray());
			}
		}
	}
}