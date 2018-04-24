using System;
using System.Diagnostics;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Model.Hosting;
using Vertica.Integration.WebApi.Infrastructure;

namespace Vertica.Integration.WebApi
{
	public class WebApiHost : IHost
    {
	    private const string Url = "url";

	    private readonly IHttpServerFactory _factory;
	    private readonly IShutdown _shutdown;

	    public WebApiHost(IHttpServerFactory factory, IShutdown shutdown)
	    {
		    _factory = factory;
		    _shutdown = shutdown;
	    }

	    public bool CanHandle(HostArguments args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

			return string.Equals(args.Command, nameof(WebApiHost), StringComparison.OrdinalIgnoreCase);
        }

        public void Handle(HostArguments args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            string url = EnsureUrl(args, out _);

			using (_factory.Create(url))
			{
				if (Environment.UserInteractive && !args.CommandArgs.Contains("noBrowser"))
					Process.Start(url);

				_shutdown.WaitForShutdown();
			}
        }
        
	    public virtual string Description => @"WebApiHost is used to host and expose all WebApi ApiControllers registred part of the initial configuration. To start this Host, use the following command: ""WebApiHost url:http://localhost:8080"" (you can choose any valid URL).";

	    private string EnsureUrl(HostArguments args, out bool basedOnSettings)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));

		    args.Args.TryGetValue(Url, out string url);

			if (string.IsNullOrWhiteSpace(url))
				return _factory.GetOrGenerateUrl(out basedOnSettings);

		    basedOnSettings = false;
			return url;
		}
    }
}