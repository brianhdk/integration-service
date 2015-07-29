using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model.Hosting.Handlers;
using Vertica.Integration.Model.Web;

namespace Vertica.Integration.Model.Hosting
{
    public class WebApiHost : IHost
    {
	    private const string Url = "url";

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
			if (!args.Args.TryGetValue(Url, out url))
				args.CommandArgs.TryGetValue(Url, out url);

	        if (String.IsNullOrWhiteSpace(url))
		        url = GenerateUrl();

	        AssertUrl(url);

			args = new HostArguments(args.Command, args.CommandArgs.ToArray(), new[]
			{
				new KeyValuePair<string, string>(Url, url) 
			});

			var windowsService = new WindowsService(this.Name(), String.Format("[URL: {0}] {1}", url, Description)).OnStart(() => _factory.Create(url));

	        if (!_windowsService.Handle(args, windowsService))
	        {
				using (_factory.Create(url))
				{
					if (Environment.UserInteractive)
						Process.Start(url);

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

		private static string GenerateUrl()
		{
			var listener = new TcpListener(IPAddress.Loopback, 0);
			listener.Start();

			string url = String.Format("http://{0}", listener.LocalEndpoint);

			listener.Stop();

			return url;
		}

		private void AssertUrl(string url)
		{
			Uri dummy;
			if (Uri.TryCreate(url, UriKind.Absolute, out dummy))
				return;

			if (Regex.IsMatch(url ?? String.Empty, @"^http(?:s)?://\+(?:\:\d+)?/?$", RegexOptions.IgnoreCase))
				return;

			throw new InvalidOperationException(String.Format("'{0}' is not a valid absolute url.", url));
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