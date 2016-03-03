using System;
using System.IO;
using System.Threading;

namespace Vertica.Integration.Infrastructure.IO
{
	public class FilesChangedHandler : IProcessExitHandler
	{
		private readonly ManualResetEvent _waitHandle;

		public FilesChangedHandler(DirectoryInfo directoryToWatch)
		{
			if (directoryToWatch == null) throw new ArgumentNullException(nameof(directoryToWatch));

			_waitHandle = new ManualResetEvent(false);

			var watcher = new FileSystemWatcher(directoryToWatch.FullName);
			watcher.Created += (sender, e) => _waitHandle.Set();
			watcher.Changed += (sender, e) => _waitHandle.Set();
			watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite;
			watcher.IncludeSubdirectories = false;
			watcher.EnableRaisingEvents = true;
		}

		public void Wait()
		{
			_waitHandle.WaitOne();
		}
	}
}