using System;
using System.Collections.Generic;
using Castle.MicroKernel;
using Hangfire;
using Hangfire.Server;

namespace Vertica.Integration.Hangfire
{
	internal class HangfireServerImpl : IDisposable
	{
		private readonly IKernel _kernel;
		private readonly IInternalConfiguration _configuration;

		private readonly BackgroundJobServer _server;

		public HangfireServerImpl(IKernel kernel, IInternalConfiguration configuration)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));

			_kernel = kernel;
			_configuration = configuration;

			Execute(_configuration.OnStartup);

			IBackgroundProcess[] backgroundProcesses = kernel.ResolveAll<IBackgroundProcess>();

            _server = new BackgroundJobServer(configuration.ServerOptions, JobStorage.Current, backgroundProcesses);
		}

		public void Dispose()
		{
			_server.Dispose();

			Execute(_configuration.OnShutdown);
		}

		private void Execute(IEnumerable<Action<IKernel>> actions)
		{
			foreach (Action<IKernel> action in actions)
				action(_kernel);
		}
	}
}