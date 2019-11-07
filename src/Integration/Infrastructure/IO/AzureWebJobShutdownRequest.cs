using System;
using System.IO;
using System.Threading;

namespace Vertica.Integration.Infrastructure.IO
{
	internal class AzureWebJobShutdownRequest : IWaitForShutdownRequest
    {
		private const string EnvironmentVariableName = "WEBJOBS_SHUTDOWN_FILE";

        private readonly ManualResetEvent _waitHandle;

        public AzureWebJobShutdownRequest()
            : this(GetDirectoryToWatch())
		{
        }

	    internal AzureWebJobShutdownRequest(DirectoryInfo directory)
	    {
	        if (directory == null) throw new ArgumentNullException(nameof(directory));

            _waitHandle = new ManualResetEvent(false);

            var watcher = new FileSystemWatcher(directory.FullName);
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

	    public static bool IsRunningInAzure()
		{
			return !string.IsNullOrEmpty(ShutdownFile());
		}

		private static DirectoryInfo GetDirectoryToWatch()
		{
			string shutdownFile = ShutdownFile();

			if (string.IsNullOrWhiteSpace(shutdownFile))
				throw new InvalidOperationException($"Environment variable {EnvironmentVariableName} is null or empty.");

			return new DirectoryInfo(Path.GetDirectoryName(shutdownFile) ?? string.Empty);
		}

		private static string ShutdownFile()
		{
			return Environment.GetEnvironmentVariable(EnvironmentVariableName);
		}
	}
}