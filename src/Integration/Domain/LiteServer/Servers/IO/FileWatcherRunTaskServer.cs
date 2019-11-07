using System.Collections.Generic;
using System.IO;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;

namespace Vertica.Integration.Domain.LiteServer.Servers.IO
{
	public abstract class FileWatcherRunTaskServer<TTask> : FileWatcherServer
		where TTask : class, ITask
	{
		private readonly ITaskFactory _factory;
		private readonly ITaskRunner _runner;

		protected FileWatcherRunTaskServer(ITaskFactory factory, ITaskRunner runner)
		{
			_factory = factory;
			_runner = runner;
		}

		protected override void ProcessFile(FileInfo file, FileSystemEventArgs args)
		{
			RunTask("File", args);
		}

		protected override void ProcessDirectory(DirectoryInfo directory, FileSystemEventArgs args)
		{
			RunTask("Directory", args);
		}

		private void RunTask(string name, FileSystemEventArgs args)
		{
			var arguments = new Arguments(
				new KeyValuePair<string, string>(name, args.FullPath), 
				new KeyValuePair<string, string>("ChangeType", args.ChangeType.ToString()));

			_runner.Execute(_factory.Get<TTask>(), arguments);
		}

	    public override string ToString()
	    {
	        return $"{base.ToString()} - {typeof(TTask).TaskName()}";
	    }
	}
}