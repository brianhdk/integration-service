using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Vertica.Integration.Domain.LiteServer.Servers.IO
{
	public abstract class FileWatcherServer : IBackgroundServer
	{
		public Task Create(CancellationToken token, BackgroundServerContext context)
		{
			DirectoryInfo path = PathToMonitor();

			if (!path.Exists)
				throw new DirectoryNotFoundException($"Cannot watch path {path.FullName}. The directory does not exist.");

			string filterLocal = Filter;
			bool includeSubDirectoriesLocal = IncludeSubDirectories;

			return Task.Run(() =>
			{
				var events = new BlockingCollection<FileSystemEventArgs>();

				try
				{
					using (var watcher = new FileSystemWatcher(path.FullName, filterLocal))
					{
						watcher.Error += (sender, args) =>
						{
							throw args.GetException();
						};

						NotifyFilters notifyFiltersLocal = watcher.NotifyFilter;
						ChangeNotifyFilters(change => notifyFiltersLocal = change);

						watcher.NotifyFilter = notifyFiltersLocal;
						watcher.IncludeSubdirectories = includeSubDirectoriesLocal;

						if (WatchCreated)
							watcher.Created += (sender, args) => events.Add(args, token);

						if (WatchRenamed)
							watcher.Renamed += (sender, args) => events.Add(args, token);

						if (WatchDeleted)
							watcher.Deleted += (sender, args) => events.Add(args, token);

						if (WatchChanged)
							watcher.Changed += (sender, args) => events.Add(args, token);

						AddManualFileSystemEventArgs(
							path, 
							watcher.Filter, 
							watcher.IncludeSubdirectories, 
							watcher.NotifyFilter, 
							fileEvent => events.Add(fileEvent, token));

						// Start watcher
						watcher.EnableRaisingEvents = true;

						while (!token.IsCancellationRequested)
						{
							FileSystemEventArgs fileEvent = events.Take(token);

							Process(fileEvent);
						}
					}
				}
				catch (OperationCanceledException)
				{
					// BlockingCollection throws this, if the Thread is canceled.
				}

			}, token);
		}

		/// <summary>
		/// (Default: True) By default we'll watch on the Created event of <see cref="FileSystemWatcher"/>
		/// </summary>
		public virtual bool WatchCreated => true;

		/// <summary>
		/// (Default: False) By default we'll not watch on the Deleted event of <see cref="FileSystemWatcher"/>
		/// </summary>
		public virtual bool WatchDeleted => false;

		/// <summary>
		/// (Default: False) By default we'll not watch on the Renamed event of <see cref="FileSystemWatcher"/>
		/// </summary>
		public virtual bool WatchRenamed => false;

		/// <summary>
		/// (Default: False) By default we'll not watch on the Changed event of <see cref="FileSystemWatcher"/>
		/// </summary>
		public virtual bool WatchChanged => false;

		/// <summary>
		/// Specifies the path used by the <see cref="FileSystemWatcher"/>.
		/// </summary>
		protected abstract DirectoryInfo PathToMonitor();

		/// <summary>
		/// This method guarantees that <see cref="FileSystemEventArgs"/> can be processed sequentially in the order they got added (FIFO).
		/// </summary>
		protected virtual void Process(FileSystemEventArgs args)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));

			var file = new FileInfo(args.FullPath);

			if (file.Exists)
			{
				ProcessFile(file, args);
				return;
			}

			var directory = new DirectoryInfo(args.FullPath);

			if (directory.Exists)
			{
				ProcessDirectory(directory, args);
				return;
			}

			ProcessPathNotExists(args);
		}

		/// <summary>
		/// Implement logic for <see cref="FileInfo"/> that has been monitored.
		/// </summary>
		protected abstract void ProcessFile(FileInfo file, FileSystemEventArgs args);

		/// <summary>
		/// Implement logic for <see cref="DirectoryInfo"/> that has been monitored.
		/// </summary>
		protected abstract void ProcessDirectory(DirectoryInfo directory, FileSystemEventArgs args);

		/// <summary>
		/// This method will be called for files/directories that has been monitored but does not exist anymore.
		/// </summary>
		protected virtual void ProcessPathNotExists(FileSystemEventArgs args)
		{
		}

		/// <summary>
		/// (Default: *.*) Specifies the filter used by the <see cref="FileSystemWatcher"/>.
		/// </summary>
		protected virtual string Filter => "*.*";

		/// <summary>
		/// (Default: False) Specifies whether sub directories should be monitored.
		/// </summary>
		protected virtual bool IncludeSubDirectories => false;

		/// <summary>
		/// Use this method to change the default NotifyFilters of <see cref="FileSystemWatcher"/>.
		/// Pass the NotifyFilters to the delegate.
		/// </summary>
		protected virtual void ChangeNotifyFilters(Action<NotifyFilters> change)
		{
		}

		/// <summary>
		/// By default existing files and directories will be added. You can change this behaviour by overriding this method.
		/// This depends on the use of <see cref="NotifyFilters"/> though. 
		/// 
		/// If you need, you can recognize the manual files added as they'll be of type <see cref="ManualFileSystemEventArgs"/>.
		/// </summary>
		protected virtual void AddManualFileSystemEventArgs(DirectoryInfo path, string filter, bool includeSubDirectories, NotifyFilters notifyFilters, Action<ManualFileSystemEventArgs> adder)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));
			if (adder == null) throw new ArgumentNullException(nameof(adder));

			foreach (
				FileSystemInfo info in
					path.EnumerateFileSystemInfos(filter,
						includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
			{
				bool isDirectory = info.Attributes.HasFlag(FileAttributes.Directory);
				bool monitorDirectoryName = notifyFilters.HasFlag(NotifyFilters.DirectoryName);

				if (isDirectory)
				{
					if (!monitorDirectoryName)
						continue;
				}
				else
				{
					bool monitorFileName = notifyFilters.HasFlag(NotifyFilters.FileName);

					if (!monitorFileName)
						continue;
				}

				adder(new ManualFileSystemEventArgs(
					WatcherChangeTypes.Created, 
					Path.GetDirectoryName(info.FullName) ?? info.FullName, 
					info.Name));
			}
		}
	}
}
