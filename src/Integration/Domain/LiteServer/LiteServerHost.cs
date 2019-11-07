using System;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Domain.LiteServer
{
	public class LiteServerHost : IHost
	{
	    private readonly ILiteServerFactory _serverFactory;
	    private readonly IShutdown _shutdown;

	    public LiteServerHost(ILiteServerFactory serverFactory, IShutdown shutdown)
		{
	        _serverFactory = serverFactory;
	        _shutdown = shutdown;
		}

		public bool CanHandle(HostArguments args)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));

			return string.Equals(args.Command, nameof(LiteServerHost), StringComparison.OrdinalIgnoreCase);
		}

		public void Handle(HostArguments args)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));

			using (_serverFactory.Create())
			{
                _shutdown.WaitForShutdown();
            }
		}
        
		public virtual string Description => @"LiteServerHost is a generic server component.";
	}
}