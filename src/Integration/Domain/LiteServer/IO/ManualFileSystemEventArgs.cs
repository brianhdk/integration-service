using System.IO;

namespace Vertica.Integration.Domain.LiteServer.IO
{
	public class ManualFileSystemEventArgs : FileSystemEventArgs
	{
		public ManualFileSystemEventArgs(WatcherChangeTypes changeType, string directory, string name)
			: base(changeType, directory, name)
		{
		}
	}
}