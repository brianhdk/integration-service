using System;
using System.IO;
using Rebus.Bus;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model.Hosting;
using Vertica.Integration.Model.Hosting.Handlers;

namespace Vertica.Integration.Rebus
{
	internal class RebusHost : IHost
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
            if (args == null) throw new ArgumentNullException("args");

            return String.Equals(args.Command, this.Name(), StringComparison.OrdinalIgnoreCase);
        }

        public void Handle(HostArguments args)
        {
            if (args == null) throw new ArgumentNullException("args");

	        var windowsService = new WindowsService(this.Name(), Description).OnStart(_bus);

	        if (!_windowsService.Handle(args, windowsService))
	        {
				using (_bus())
				{
					do
					{
						_outputter.WriteLine(@"Press ESCAPE to stop Rebus...");
						_outputter.WriteLine();

					} while (WaitingForEscape());
				}
	        }
        }

        public string Description
        {
            get { return "Hosts Rebus"; }
        }

		private static bool WaitingForEscape()
		{
			// We can't do anything but to return true.
			if (!Environment.UserInteractive)
				return true;

			return Console.ReadKey(intercept: true /* don't display */).Key != ConsoleKey.Escape;
		}
    }
}