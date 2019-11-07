using System;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Hangfire
{
	public class HangfireHost : IHost
	{
	    private readonly IHangfireServerFactory _serverFactory;
	    private readonly IShutdown _shutdown;

	    public HangfireHost(IHangfireServerFactory serverFactory, IShutdown shutdown)
		{
	        _serverFactory = serverFactory;
	        _shutdown = shutdown;
		}

		public bool CanHandle(HostArguments args)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));

			return string.Equals(args.Command, nameof(HangfireHost), StringComparison.OrdinalIgnoreCase);
		}

		public void Handle(HostArguments args)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));

			using (_serverFactory.Create())
			{
				_shutdown.WaitForShutdown();
			}
		}

		public string Description => "Hangfire host.";
	}
}