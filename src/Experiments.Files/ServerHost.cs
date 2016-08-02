using System;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Model.Hosting;
using Vertica.Integration.Model.Hosting.Handlers;

namespace Experiments.Files
{
	internal class ServerHost : IHost
	{
		private readonly IWindowsServiceHandler _windowsService;
		private readonly IProcessExitHandler _processExit;
		private readonly IServerFactory _serverFactory;

		public ServerHost(IWindowsServiceHandler windowsService, IProcessExitHandler processExit, IServerFactory serverFactory)
		{
			_windowsService = windowsService;
			_processExit = processExit;
			_serverFactory = serverFactory;
		}

		public bool CanHandle(HostArguments args)
		{
			// TODO
			return true;
		}

		public void Handle(HostArguments args)
		{
			if (InstallOrRunAsWindowsService(args, Create))
				return;

			using (Create())
			{
				_processExit.Wait();
			}

			// start X-servere op
			// tillad registrering af egne "servere"
			// fx WebApi
			// fx Rebus
			// fx hMail
			// implementer en server som kan:
			// registrere fil-watchers
			// registrere sin egen fil-watcher baseret på konfiguration
			// som afvikler tasks
			// registrere baggrundstråde
			// som kan køre hvert X-sekund
		}

		private IDisposable Create()
		{
			return _serverFactory.Create();
		}

		private bool InstallOrRunAsWindowsService(HostArguments args, Func<IDisposable> factory)
		{
			return _windowsService.Handle(args, new HandleAsWindowsService(this.Name(), this.Name(), Description, factory));
		}

		public string Description => "TBD";
	}
}