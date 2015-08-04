using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model.Hosting;
using Vertica.Integration.Model.Hosting.Handlers;
using Vertica.Integration.WebApi.Infrastructure;

namespace Vertica.Integration.WebApi
{
    public class WebApiHost : IHost
    {
	    internal static readonly string Command = typeof (WebApiHost).HostName();

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

			return String.Equals(args.Command, Command, StringComparison.OrdinalIgnoreCase);
        }

        public void Handle(HostArguments args)
        {
            if (args == null) throw new ArgumentNullException("args");

	        string url = ParseUrl(args);

			args = new HostArguments(args.Command, args.CommandArgs.ToArray(), new[]
			{
				WithUrl(url) 
			});

			var windowsService = new WindowsService(this.Name(), this.WindowsServiceDescription(url)).OnStart(() => _factory.Create(url));

	        if (!_windowsService.Handle(args, windowsService))
	        {
				using (_factory.Create(url))
				{
					if (Environment.UserInteractive && !args.CommandArgs.Contains("noBrowser"))
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
			get { return HostDescription; }
        }

	    internal static string HostDescription
	    {
		    get
		    {
			    return @"WebApiHost is used to host and expose all WebApi ApiControllers registred part of the initial configuration. To start this Host, use the following command: ""WebApiHost url:http://localhost:8080"" (you can choose any valid URL).";
		    }
	    }

	    internal static string EnsureUrl(string url)
	    {
			if (String.IsNullOrWhiteSpace(url))
				url = GenerateUrl();

		    return url;
	    }

	    internal static KeyValuePair<string, string> WithUrl(string url)
	    {
		    AssertUrl(url);
			return new KeyValuePair<string, string>(Url, url);
		}

	    private static string ParseUrl(HostArguments args)
		{
			if (args == null) throw new ArgumentNullException("args");

			string url;
			args.Args.TryGetValue(Url, out url);

			return EnsureUrl(url);
		}

	    private static void AssertUrl(string url)
		{
			Uri dummy;
			if (Uri.TryCreate(url, UriKind.Absolute, out dummy))
				return;

			if (Regex.IsMatch(url ?? String.Empty, @"^http(?:s)?://\+(?:\:\d+)?/?$", RegexOptions.IgnoreCase))
				return;

			throw new InvalidOperationException(String.Format("'{0}' is not a valid absolute url.", url));
		}

	    private static string GenerateUrl()
		{
			var listener = new TcpListener(IPAddress.Loopback, 0);
			listener.Start();

			string url = String.Format("http://{0}", listener.LocalEndpoint);

			listener.Stop();

			return url;
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