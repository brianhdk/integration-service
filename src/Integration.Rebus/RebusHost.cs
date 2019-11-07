using System;
using Rebus.Bus;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Rebus
{
	public class RebusHost : IHost
    {
	    private readonly Func<IBus> _bus;
		private readonly IShutdown _shutdown;

	    public RebusHost(Func<IBus> bus, IShutdown shutdown)
	    {
	        _bus = bus;
	        _shutdown = shutdown;
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

			_shutdown.WaitForShutdown();
        }

		public string Description => "Hosts Rebus";
    }
}