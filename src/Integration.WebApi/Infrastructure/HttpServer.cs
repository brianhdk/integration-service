using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using Castle.MicroKernel;
using Microsoft.Owin.Hosting;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.IO;

namespace Vertica.Integration.WebApi.Infrastructure
{
    internal class HttpServer : IDisposable
    {
	    private readonly IConsoleWriter _console;
	    private readonly IDisposable _httpServer;

	    public HttpServer(IKernel kernel, Action<IOwinConfiguration> configuration, string url)
        {
	        if (kernel == null) throw new ArgumentNullException(nameof(kernel));
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));
			AssertUrl(url);

	        _console = kernel.Resolve<IConsoleWriter>();

            Output($"Starting HttpServer listening on URL: {url}");

			// TODO: Make it possible to add multiple URL's to listen on
	        _httpServer = WebApp.Start(new StartOptions(url), builder =>
            {
                // TODO: Look into what this does exactly
				builder.Properties["host.TraceOutput"] = kernel.Resolve<TextWriter>();

                builder.Configure(kernel, cfg =>
                {
                    ConcurrentDictionary<object, object> properties = cfg.Http.Properties;
                    properties["host.OnAppDisposing"] = kernel.Resolve<IShutdown>().Token;

                    configuration(cfg);
                });
            });
        }

		private static void AssertUrl(string url)
		{
			Uri dummy;
			if (Uri.TryCreate(url, UriKind.Absolute, out dummy))
				return;

			if (Regex.IsMatch(url ?? string.Empty, @"^http(?:s)?://\+(?:\:\d+)?/?$", RegexOptions.IgnoreCase))
				return;

			throw new ArgumentOutOfRangeException(nameof(url), url, $@"'{url}' is not a valid absolute url.");
		}

        public void Dispose()
        {
            if (_httpServer != null)
            {
	            Output("Stopping");

	            _httpServer.Dispose();

                Output("Stopped");
            }
        }
        
        private void Output(string message)
        {
            _console.WriteLine($"[HttpServer]: {message}.");
        }
    }
}