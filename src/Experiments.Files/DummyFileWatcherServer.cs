using System;
using System.IO;
using System.Threading;
using Vertica.Integration.Domain.LiteServer.IO;

namespace Experiments.Files
{
	public class DummyFileWatcherServer : FileWatcherServer
	{
		private readonly TextWriter _writer;

		public DummyFileWatcherServer(TextWriter writer)
		{
			_writer = writer;
		}

		protected override DirectoryInfo PathToMonitor()
		{
			return new DirectoryInfo(@"c:\tmp\watcher");
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