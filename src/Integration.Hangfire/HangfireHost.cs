using System;
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
		private readonly IHangfireServerFactory _serverFactory;

		public HangfireHost(IWindowsServiceHandler windowsService, IProcessExitHandler processExit, IHangfireServerFactory serverFactory)
		{
			_windowsService = windowsService;
			_processExit = processExit;
			_serverFactory = serverFactory;
		}

		public bool CanHandle(HostArguments args)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));

			return string.Equals(args.Command, Command, StringComparison.OrdinalIgnoreCase);
		}

		public void Handle(HostArguments args)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));

			if (InstallOrRunAsWindowsService(args, Create))
				return;

			using (Create())
			{
				_processExit.Wait();
			}
		}

		private IDisposable Create()
		{
			return _serverFactory.Create();
		}

		private bool InstallOrRunAsWindowsService(HostArguments args, Func<IDisposable> factory)
		{
			return _windowsService.Handle(args, new HandleAsWindowsService(this.Name(), this.Name(), Description, factory));
		}

		public string Description => "Hangfire host.";
	}
}