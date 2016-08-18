using System.IO;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Domain.LiteServer.Servers.IO;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Vertica.Integration.WebApi;
using Task = Vertica.Integration.Model.Task;

namespace Experiments.LiteServer
{
	class Program
	{
		static void Main(string[] args)
		{
			using (IApplicationContext context = ApplicationContext.Create(application => application
				.Database(database => database.DisableIntegrationDb())
				.Tasks(tasks => tasks.AddFromAssemblyOfThis<Program>())
				.UseWebApi(webapi => webapi.AddToLiteServer())
				.UseLiteServer(server => server
					.AddWorker<CustomWorker>()
					.AddServer<FileMonitorServer>()
					.AddServer<ExecuteTaskWhenFileIsAddedServer>())))
			{
				context.Execute(typeof(LiteServerHost).HostName());
			}
		}
	}

	internal class ExecuteTaskWhenFileIsAddedServer : FileWatcherRunTaskServer<ImportToPerfionTask>
	{
		public ExecuteTaskWhenFileIsAddedServer(ITaskFactory factory, ITaskRunner runner) : base(factory, runner)
		{
		}

		protected override DirectoryInfo PathToMonitor()
		{
			return new DirectoryInfo(@"c:\tmp\watchthis2");
		}
	}

	public class ImportToPerfionTask : Task
	{
		private readonly TextWriter _writer;

		public ImportToPerfionTask(TextWriter writer)
		{
			_writer = writer;
		}

		public override string Description => "This Task imports to Perfion....";

		public override void StartTask(ITaskExecutionContext context)
		{
			string file = context.Arguments["File"];

			_writer.WriteLine($"[Task]: {file}");
		}
	}

	internal class FileMonitorServer : FileWatcherServer
	{
		private readonly TextWriter _writer;

		public FileMonitorServer(TextWriter writer)
		{
			_writer = writer;
		}

		protected override DirectoryInfo PathToMonitor()
		{
			return new DirectoryInfo(@"c:\tmp\watchthis");
		}

		protected override void ProcessFile(FileInfo file, FileSystemEventArgs args)
		{
			_writer.WriteLine($"[File]: {file.Name}");
		}

		protected override void ProcessDirectory(DirectoryInfo directory, FileSystemEventArgs args)
		{
			_writer.WriteLine($"[Directory]: {directory.Name}");
		}
	}
}
