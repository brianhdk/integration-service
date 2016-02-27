using Hangfire;

namespace Vertica.Integration.Hangfire
{
	internal interface IInternalConfiguration
	{
		BackgroundJobServerOptions ServerOptions { get; }
		StartupActions OnStartup { get; }
		ShutdownActions OnShutdown { get; }
	}
}