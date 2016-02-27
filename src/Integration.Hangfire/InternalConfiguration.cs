using Hangfire;

namespace Vertica.Integration.Hangfire
{
	internal class InternalConfiguration : IInternalConfiguration
	{
		public InternalConfiguration()
		{
			ServerOptions = new BackgroundJobServerOptions();
			OnStartup = new StartupActions();
			OnShutdown = new ShutdownActions();
		}

		public BackgroundJobServerOptions ServerOptions { get; }
		public StartupActions OnStartup { get; }
		public ShutdownActions OnShutdown { get; }
	}
}