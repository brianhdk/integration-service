using System;
using System.Collections.Generic;
using Castle.MicroKernel;
using Hangfire;
using Hangfire.Server;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Model.Hosting;
using Vertica.Integration.Model.Hosting.Handlers;

namespace Vertica.Integration.Hangfire
{
	public class HangfireHost : IHost
	{
		internal static readonly string Command = typeof(HangfireHost).HostName();

		private readonly IWindowsServiceHandler _windowsService;
		private readonly IProcessExitHandler _processExit;
		private readonly IInternalConfiguration _configuration;
		private readonly IKernel _kernel;

		public HangfireHost(IWindowsServiceHandler windowsService, IProcessExitHandler processExit, IKernel kernel)
		{
			_windowsService = windowsService;
			_processExit = processExit;
			_kernel = kernel;
			_configuration = kernel.Resolve<IInternalConfiguration>();
		}

		public bool CanHandle(HostArguments args)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));

			return string.Equals(args.Command, Command, StringComparison.OrdinalIgnoreCase);
		}

		public void Handle(HostArguments args)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));

			if (InstallOrRunAsWindowsService(args, Initialize))
				return;

			using (Initialize())
			{
				_processExit.Wait();
			}
		}

		private IDisposable Initialize()
		{
			return new HangfireServer(_configuration, _kernel);
		}

		private bool InstallOrRunAsWindowsService(HostArguments args, Func<IDisposable> factory)
		{
			return _windowsService.Handle(args, new HandleAsWindowsService(this.Name(), this.Name(), Description, factory));
		}

		public string Description => "Hangfire host.";

		private class HangfireServer : IDisposable
		{
			private readonly IInternalConfiguration _configuration;
			private readonly IKernel _kernel;
			private readonly BackgroundJobServer _server;

			public HangfireServer(IInternalConfiguration configuration, IKernel kernel)
			{
				_configuration = configuration;
				_kernel = kernel;

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
}