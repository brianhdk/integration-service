using System;
using Rebus.Bus;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Model.Hosting;
using Vertica.Integration.Model.Hosting.Handlers;

namespace Vertica.Integration.Rebus
{
	public class RebusHost : IHost
    {
	    private readonly Func<IBus> _bus;
	    private readonly IWindowsServiceHandler _windowsService;
		private readonly IProcessExitHandler _processExit;

	    public RebusHost(Func<IBus> bus, IWindowsServiceHandler windowsService, IProcessExitHandler processExit)
	    {
		    _windowsService = windowsService;
		    _processExit = processExit;
		    _bus = bus;
	    }

	    public bool CanHandle(HostArguments args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            return string.Equals(args.Command, this.Name(), StringComparison.OrdinalIgnoreCase);
        }

        public void Handle(HostArguments args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

			// Initialize Rebus
			_bus();

	        var windowsService = new HandleAsWindowsService(this.Name(), this.Name(), Description);

	        if (!_windowsService.Handle(args, windowsService))
				_processExit.Wait();
        }

		public string Description => "Hosts Rebus";
    }
}