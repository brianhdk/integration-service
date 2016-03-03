using System;
using System.IO;
using System.Threading;

namespace Vertica.Integration.Infrastructure.IO
{
	public class FilesChangedHandler : IProcessExitHandler
	{
		private readonly ManualResetEvent _waitHandle;

		public FilesChangedHandler(FileInfo fileToWatch)
			: this(fileToWatch?.FullName)
		{
		}

		public FilesChangedHandler(DirectoryInfo directoryToWatch)
			: this(directoryToWatch?.FullName)
		{
		}

		private FilesChangedHandler(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException(@"Value cannot be null or empty", nameof(path));
			
			_waitHandle = new ManualResetEvent(false);

			var watcher = new FileSystemWatcher(path);
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