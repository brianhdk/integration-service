using System;
using System.IO;
using System.Threading;
using Vertica.Integration.Domain.LiteServer.IO;

namespace Experiments.Files
{
	public class DummyFileWatcherServer : FileWatcherServer
	{
		private readonly TextWriter _writer;
		private readonly DirectoryInfo _pathToMonitor;

		public DummyFileWatcherServer(TextWriter writer)
		{
			_writer = writer;
			_pathToMonitor = new DirectoryInfo(@"c:\tmp\watcher");
			_writer.WriteLine("Monitor: {0}", _pathToMonitor);
		}

		protected override DirectoryInfo PathToMonitor()
		{
			return _pathToMonitor;
		}

		protected override bool IncludeSubDirectories => false;

		protected override void ProcessFile(FileInfo file, FileSystemEventArgs args)
		{
			_writer.WriteLine($"[FILE] {file.Name} [{args.ChangeType}]");

			if (!string.Equals(file.Name, "test.txt", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					file.Delete();
					Thread.Sleep(100);
				}
				catch (Exception ex)
				{
					_writer.WriteLine(ex.Message);
				}
			}
		}

		protected override void ProcessDirectory(DirectoryInfo directory, FileSystemEventArgs args)
		{
			_writer.WriteLine($"[DIRECTORY] {directory.Name} [{args.ChangeType}]");

			if (!string.Equals(directory.Name, "test", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					directory.Delete(true);
					Thread.Sleep(100);
				}
				catch (Exception ex)
				{
					_writer.WriteLine(ex.Message);
				}
			}
		}
	}
}