using System;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model.Hosting;
using Vertica.Integration.Model.Hosting.Handlers;

namespace Vertica.Integration.Domain.LiteServer
{
	public class LiteServerHost : IHost
	{
		internal static readonly string Command = typeof(LiteServerHost).HostName();

		private readonly IWindowsServiceHandler _windowsService;
	    private readonly ILiteServerFactory _serverFactory;
	    private readonly IShutdown _shutdown;

	    public LiteServerHost(IWindowsServiceHandler windowsService, ILiteServerFactory serverFactory, IShutdown shutdown)
		{
			_windowsService = windowsService;
	        _serverFactory = serverFactory;
	        _shutdown = shutdown;
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
				_shutdown.WaitForShutdown();
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