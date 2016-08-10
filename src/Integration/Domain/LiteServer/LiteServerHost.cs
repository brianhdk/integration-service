using System;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Model.Hosting;
using Vertica.Integration.Model.Hosting.Handlers;

namespace Vertica.Integration.Domain.LiteServer
{
	public class LiteServerHost : IHost
	{
		internal static readonly string Command = typeof(LiteServerHost).HostName();

		private readonly IWindowsServiceHandler _windowsService;
		private readonly IProcessExitHandler _processExit;
		private readonly ILiteServerFactory _serverFactory;

		public LiteServerHost(IWindowsServiceHandler windowsService, IProcessExitHandler processExit, ILiteServerFactory serverFactory)
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

		public virtual string Description => @"LiteServerHost is a generic server component.";
	}
}