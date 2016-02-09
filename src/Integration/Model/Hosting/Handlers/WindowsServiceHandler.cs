using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using Vertica.Integration.Infrastructure.Windows;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class WindowsServiceHandler : IWindowsServiceHandler
	{
		private const string Command = "service";

		internal const string ServiceStartMode = "startmode";
		internal const string ServiceAccountCommand = "account";
		internal const string ServiceAccountUsernameCommand = "username";
		internal const string ServiceAccountPasswordCommand = "password";

	    private readonly IRuntimeSettings _runtimeSettings;
		private readonly IWindowsServices _windowsServices;
	    
	    public WindowsServiceHandler(IRuntimeSettings runtimeSettings, IWindowsFactory windows)
	    {
		    if (runtimeSettings == null) throw new ArgumentNullException("runtimeSettings");

		    _runtimeSettings = runtimeSettings;
			_windowsServices = windows.WindowsServices();
	    }

        public bool Handle(HostArguments args, HandleAsWindowsService service)
        {
            if (args == null) throw new ArgumentNullException("args");
	        if (service == null) throw new ArgumentNullException("service");

	        string action;
			if (!args.CommandArgs.TryGetValue(Command, out action))
                return false;

	        Func<KeyValuePair<string, string>, bool> actionIs = command =>
		        String.Equals(command.Value, action, StringComparison.OrdinalIgnoreCase);

			if (actionIs(InstallCommand))
			{
				var configuration = new WindowsServiceConfiguration(GetServiceName(service), ExePath, ServiceArgs(args))
					.DisplayName(Prefix(service.DisplayName))
					.Description(service.Description);

				string startMode;
				args.CommandArgs.TryGetValue(ServiceStartMode, out startMode);

				ServiceStartMode serviceStartMode;
				if (Enum.TryParse(startMode, true, out serviceStartMode))
					configuration.StartMode(serviceStartMode);

				string account;
				args.CommandArgs.TryGetValue(ServiceAccountCommand, out account);

				ServiceAccount serviceAccount;
				if (Enum.TryParse(account, true, out serviceAccount))
					configuration.WithAccount(serviceAccount);

				string username;
				args.CommandArgs.TryGetValue(ServiceAccountUsernameCommand, out username);

				string password;
				args.CommandArgs.TryGetValue(ServiceAccountPasswordCommand, out password);

				if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
					configuration.WithCredentials(username, password);

				_windowsServices.Install(configuration);
			}
			else if (actionIs(UninstallCommand))
			{
				_windowsServices.Uninstall(GetServiceName(service));
			}
			else
			{
				_windowsServices.Run(GetServiceName(service), service.OnStartFactory);
			}		        

            return true;
        }

		private static string ExePath
		{
			get { return Assembly.GetEntryAssembly().Location; }
		}

		private string GetServiceName(HandleAsWindowsService service)
		{
			if (service == null) throw new ArgumentNullException("service");

			return Regex.Replace(Prefix(service.Name), @"\W", String.Empty);
		}

		private string Prefix(string value)
		{
			ApplicationEnvironment environment = _runtimeSettings.Environment;

			return String.Format("Integration Service{0}: {1}",
				environment != null
					? String.Format(" [{0}]", environment)
					: String.Empty,
				value);
		}

		private static string ServiceArgs(HostArguments args)
		{
			Arguments arguments = new Arguments(args.CommandArgs
				.Where(x => !ReservedCommandArgs.Contains(x.Key, StringComparer.OrdinalIgnoreCase))
				.Append(new KeyValuePair<string, string>("-service", String.Empty))
				.Append(args.Args.ToArray())
				.ToArray());

			return String.Format("{0} {1}", args.Command, arguments);
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

		public static KeyValuePair<string, string> InstallCommand
		{
			get { return new KeyValuePair<string, string>(Command, "install"); }
		}

		public static KeyValuePair<string, string> UninstallCommand
		{
			get { return new KeyValuePair<string, string>(Command, "uninstall"); }
		}
    }
}