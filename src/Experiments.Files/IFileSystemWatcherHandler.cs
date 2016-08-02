using System.IO;

namespace Experiments.Files
{
	public interface IFileSystemWatcherHandler
	{
		void Initialize(FileSystemWatcher watcher);
		void Handle(FileSystemEventArgs args);
	}
}