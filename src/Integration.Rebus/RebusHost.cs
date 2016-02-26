using System;
using System.IO;
using Rebus.Bus;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model.Hosting;
using Vertica.Integration.Model.Hosting.Handlers;

namespace Vertica.Integration.Rebus
{
	public class RebusHost : IHost
    {
	    private readonly Func<IBus> _bus;
	    private readonly IWindowsServiceHandler _windowsService;
	    private readonly TextWriter _outputter;

	    public RebusHost(Func<IBus> bus, IWindowsServiceHandler windowsService, TextWriter outputter)
	    {
		    _windowsService = windowsService;
		    _outputter = outputter;
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
				_outputter.WaitUntilEscapeKeyIsHit(@"Press ESCAPE to stop Rebus...");
        }

		public string Description => "Hosts Rebus";
    }
}