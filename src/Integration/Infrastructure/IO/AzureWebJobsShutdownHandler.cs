using System;
using System.IO;

namespace Vertica.Integration.Infrastructure.IO
{
	public class AzureWebJobsShutdownHandler : FilesChangedHandler
	{
		public AzureWebJobsShutdownHandler()
			: base(GetDirectoryToWatch())
		{
		}

		private static DirectoryInfo GetDirectoryToWatch()
		{
			const string name = "WEBJOBS_SHUTDOWN_FILE";

			string shutdownFile = Environment.GetEnvironmentVariable(name);

			if (string.IsNullOrWhiteSpace(shutdownFile))
				throw new InvalidOperationException($"Environment variable {name} is null or empty.");

			return new DirectoryInfo(Path.GetDirectoryName(shutdownFile) ?? string.Empty);
		}
	}
}