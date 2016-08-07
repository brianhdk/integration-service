using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vertica.Integration.Domain.LiteServer;

namespace Experiments.Files
{
	internal class DummyFileWatcherOperation : IBackgroundServer
	{
		public Task Create(CancellationToken token)
		{
			return Task.Run(() =>
			{
				using (var watcher = new FileSystemWatcher(@"C:\tmp\watcher"))
				{
					watcher.Created += (sender, e) => Console.WriteLine("created " + e.Name + " - " + e.ChangeType + " - " + e.FullPath);

					watcher.NotifyFilter = NotifyFilters.FileName;
					watcher.IncludeSubdirectories = false;
					watcher.EnableRaisingEvents = true;

					token.WaitHandle.WaitOne();
				}

			}, token);
		}
	}
}