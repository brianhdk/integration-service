using System;
using System.IO;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model.Hosting.Handlers;
using Vertica.Integration.Model.Web;

namespace Vertica.Integration.Model.Hosting
{
    public class WebApiHost : IHost
    {
	    private readonly IWindowsServiceHandler _windowsService;
	    private readonly IHttpServerFactory _factory;
	    private readonly TextWriter _outputter;

	    public WebApiHost(IWindowsServiceHandler windowsService, IHttpServerFactory factory, TextWriter outputter)
	    {
		    _windowsService = windowsService;
		    _factory = factory;
		    _outputter = outputter;
	    }

	    public bool CanHandle(HostArguments args)
        {
            if (args == null) throw new ArgumentNullException("args");

            return String.Equals(args.Command, this.Name(), StringComparison.OrdinalIgnoreCase);
        }

        public void Handle(HostArguments args)
        {
            if (args == null) throw new ArgumentNullException("args");

			string url;
	        if (!args.CommandArgs.TryGetValue("url", out url))
		        args.Args.TryGetValue("url", out url);

			// TODO: validation of url - or use a default url if none is specified.

			/*
                Func<string, bool> parser = value =>
                {
                    Uri result;
                    return
                        Uri.TryCreate(value, UriKind.Absolute, out result) ||
                        Regex.IsMatch(value ?? String.Empty, @"^http(?:s)?://\+(?:\:\d+)?/?$", RegexOptions.IgnoreCase);
                };

                Func<string, string> error = value => String.Format("'{0}' is not a valid absolute url.", value);
			*/

			var windowsService = new WindowsService(this.Name(), Description).OnStart(() => _factory.Create(url));

	        if (!_windowsService.Handle(args, windowsService))
	        {
				using (_factory.Create(url))
				{
					do
					{
						_outputter.WriteLine("Starting web-service listening on URL: {0}", url);
						_outputter.WriteLine();
						_outputter.WriteLine(@"Press ESCAPE to stop web-service...");
						_outputter.WriteLine();

					} while (WaitingForEscape());
				}
	        }
        }

        public string Description
        {
			get { return @"WebApiHost is used to host and expose all WebApi ApiControllers registred part of the initial configuration. To start this Host, use the following command: ""WebApiHost -url:http://localhost:8080"" (you can choose any valid URL)."; }
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