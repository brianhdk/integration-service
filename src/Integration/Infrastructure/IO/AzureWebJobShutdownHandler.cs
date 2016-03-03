using System;
using System.IO;

namespace Vertica.Integration.Infrastructure.IO
{
	internal class AzureWebJobShutdownHandler : FilesChangedHandler
	{
		private const string EnvironmentVariableName = "WEBJOBS_SHUTDOWN_FILE";

		public AzureWebJobShutdownHandler()
			: base(GetDirectoryToWatch())
		{
		}

		public static bool IsRunningOnAzure()
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