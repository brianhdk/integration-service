using System;
using System.IO;
using Rebus.Bus;
using Rebus.CastleWindsor;
using Rebus.Config;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model.Hosting;
using Vertica.Integration.Model.Hosting.Handlers;

namespace Vertica.Integration.Rebus
{
    public class RebusHost : IHost
    {
        // Rebus                        => Run in Console
        // Rebus -service               => Run as Service
        // Rebus -service:install       => Install as Service
        // Rebus -service:uninstall     => Uninstall Service

	    private readonly RebusConfiguration _configuration;
	    private readonly IWindowsServiceHandler _windowsService;
	    private readonly TextWriter _outputter;

	    public RebusHost(RebusConfiguration configuration, IWindowsServiceHandler windowsService, TextWriter outputter)
	    {
		    _windowsService = windowsService;
		    _outputter = outputter;
		    _configuration = configuration;
	    }

	    public bool CanHandle(HostArguments args)
        {
            if (args == null) throw new ArgumentNullException("args");

            return String.Equals(args.Command, this.Name(), StringComparison.OrdinalIgnoreCase);
        }

        public void Handle(HostArguments args)
        {
            if (args == null) throw new ArgumentNullException("args");

	        Func<IBus> startBus = () =>
	        {
		        RebusConfigurer rebus = Configure.With(new CastleWindsorContainerAdapter(_configuration.Container));

		        return _configuration.BusConfiguration(rebus).Start();
	        };

	        WindowsService windowsService = new WindowsService(this.Name(), Description).OnStart(startBus);
	        if (!_windowsService.Handle(args, windowsService))

	        using (startBus())
	        {
				do
				{
					_outputter.WriteLine(@"Press ESCAPE to stop Rebus...");
					_outputter.WriteLine();

				} while (WaitingForEscape());
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