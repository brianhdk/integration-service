using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.IO;
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
	    private readonly IProcessExitHandler _processExit;

	    public WebApiHost(IWindowsServiceHandler windowsService, IHttpServerFactory factory, IProcessExitHandler processExit)
	    {
		    _windowsService = windowsService;
		    _factory = factory;
		    _processExit = processExit;
	    }

	    public bool CanHandle(HostArguments args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

			return string.Equals(args.Command, Command, StringComparison.OrdinalIgnoreCase);
        }

        public void Handle(HostArguments args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

	        bool basedOnSettings;
	        string url = EnsureUrl(args, out basedOnSettings);

	        KeyValuePair<string, string>[] additionalArgs = basedOnSettings
		        ? new KeyValuePair<string, string>[0]
		        : new[] { new KeyValuePair<string, string>(Url, url) };

	        args = new HostArguments(args.Command, args.CommandArgs.ToArray(), additionalArgs);

	        if (InstallOrRunAsWindowsService(args, url, basedOnSettings))
		        return;

			using (_factory.Create(url))
			{
				if (Environment.UserInteractive && !args.CommandArgs.Contains("noBrowser"))
					Process.Start(url);

				_processExit.Wait();
			}
        }

		private bool InstallOrRunAsWindowsService(HostArguments args, string url, bool basedOnSettings)
		{
			return _windowsService.Handle(
				args, 
				new HandleAsWindowsService(
					WindowsServiceName,
					WindowsServiceDisplayName,
					$"[URL: {(basedOnSettings ? "(configuration value)" : url)}] {Description}", 
					() => _factory.Create(url)));
		}

	    protected virtual string WindowsServiceName => this.Name();
	    protected virtual string WindowsServiceDisplayName => this.Name();

	    public virtual string Description => @"WebApiHost is used to host and expose all WebApi ApiControllers registred part of the initial configuration. To start this Host, use the following command: ""WebApiHost url:http://localhost:8080"" (you can choose any valid URL).";

	    private string EnsureUrl(HostArguments args, out bool basedOnSettings)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));

			string url;
			args.Args.TryGetValue(Url, out url);

			if (string.IsNullOrWhiteSpace(url))
				return _factory.GetOrGenerateUrl(out basedOnSettings);

		    basedOnSettings = false;
			return url;
		}
    }
}