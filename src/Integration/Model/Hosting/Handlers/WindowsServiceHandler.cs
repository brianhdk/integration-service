using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using Vertica.Integration.Infrastructure.Windows;
using Vertica.Utilities.Extensions.EnumerableExt;
using Vertica.Utilities.Extensions.StringExt;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class WindowsServiceHandler : IWindowsServiceHandler
	{
		private const string Command = "service";

		private const string ServiceStartMode = "startmode";
		private const string ServiceAccountCommand = "account";
		private const string ServiceAccountUsernameCommand = "username";
		private const string ServiceAccountPasswordCommand = "password";

	    private readonly IRuntimeSettings _runtimeSettings;
		private readonly IWindowsServices _windowsServices;
	    
	    public WindowsServiceHandler(IRuntimeSettings runtimeSettings, IWindowsFactory windows)
	    {
		    _runtimeSettings = runtimeSettings;
	        _windowsServices = windows.WindowsServices();
	    }

        public bool Handle(HostArguments args, HandleAsWindowsService service)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
	        if (service == null) throw new ArgumentNullException(nameof(service));

            if (!args.CommandArgs.TryGetValue(Command, out string action))
                return false;

            bool ActionIs(KeyValuePair<string, string> command) => 
                string.Equals(command.Value, action, StringComparison.OrdinalIgnoreCase);

            if (ActionIs(InstallCommand))
			{
				var configuration = new WindowsServiceConfiguration(GetServiceName(service), ExePath, ExeArgs(args))
					.DisplayName(Prefix(service.DisplayName))
					.Description(service.Description);

			    args.CommandArgs.TryGetValue(ServiceStartMode, out string startMode);

			    if (Enum.TryParse(startMode, true, out ServiceStartMode serviceStartMode))
					configuration.StartMode(serviceStartMode);

			    if (args.CommandArgs.TryGetValue(ServiceAccountCommand, out string account))
				{
				    if (Enum.TryParse(account, true, out ServiceAccount serviceAccount))
						configuration.RunAs(serviceAccount);
				}
				else
				{
				    args.CommandArgs.TryGetValue(ServiceAccountUsernameCommand, out string username);
				    args.CommandArgs.TryGetValue(ServiceAccountPasswordCommand, out string password);

					if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
						configuration.RunAsUser(username, password);
				}

				_windowsServices.Install(configuration);
			}
			else if (ActionIs(UninstallCommand))
			{
				_windowsServices.Uninstall(GetServiceName(service));
			}
			else
			{
                using (var serviceBase = new CustomService(GetServiceName(service), service.OnStartFactory))
                {
                    ServiceBase.Run(serviceBase);
                }
			}

            return true;
        }

	    private string GetServiceName(HandleAsWindowsService service)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));

			return Regex.Replace(Prefix(service.Name), @"\W", string.Empty);
		}

		private string Prefix(string value)
		{
		    bool.TryParse(_runtimeSettings["WindowsService.DontPrefix"], out bool dontPrefix);

			if (dontPrefix)
				return value;

		    var prefixes = new[]
		    {
		        _runtimeSettings.ApplicationName.NullIfEmpty(),
                _runtimeSettings.InstanceName.NullIfEmpty(),
                _runtimeSettings.Environment?.ToString()
		    }.SkipNulls().Select(prefix => $"[{prefix}]");

			return $"Integration Service{string.Join("", prefixes)}: {value}";
		}

		private static string ExePath => Assembly.GetEntryAssembly().Location;

		private static string ExeArgs(HostArguments args)
		{
			Arguments arguments = new Arguments(args.CommandArgs
				.Where(x => !ReservedCommandArgs.Contains(x.Key, StringComparer.OrdinalIgnoreCase))
				.Append(new KeyValuePair<string, string>("-service", string.Empty))
				.Append(args.Args.ToArray())
				.ToArray());

			return $"{args.Command} {arguments}";
		}

		private static IEnumerable<string> ReservedCommandArgs
		{
			get
			{
				yield return Command;
				yield return ServiceStartMode;
				yield return ServiceAccountCommand;
				yield return ServiceAccountUsernameCommand;
				yield return ServiceAccountPasswordCommand;
			}
		}

		public static KeyValuePair<string, string> InstallCommand => new KeyValuePair<string, string>(Command, "install");
		public static KeyValuePair<string, string> UninstallCommand => new KeyValuePair<string, string>(Command, "uninstall");

        private class CustomService : ServiceBase
        {
            private readonly Func<IDisposable> _onStartFactory;
            private IDisposable _current;

            public CustomService(string serviceName, Func<IDisposable> onStartFactory)
            {
                _onStartFactory = onStartFactory;
                ServiceName = serviceName;
            }

            protected override void OnStart(string[] args)
            {
                _current = _onStartFactory();
            }

            protected override void OnStop()
            {
                _current?.Dispose();
            }
        }
    }
}