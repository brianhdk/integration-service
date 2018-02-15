using System;
using System.Collections.Generic;
using Castle.MicroKernel;
using Hangfire;

namespace Vertica.Integration.Hangfire
{
	internal interface IInternalConfiguration : IEnumerable<Action<IGlobalConfiguration, IKernel>>
    {
		BackgroundJobServerOptions ServerOptions { get; }
		StartupActions OnStartup { get; }
		ShutdownActions OnShutdown { get; }
    }
}