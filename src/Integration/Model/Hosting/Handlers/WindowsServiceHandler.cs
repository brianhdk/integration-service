using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class WindowsServiceHandler : IWindowsServiceHandler
	{
		private const string ServiceCommand = "service";

		internal const string ServiceAccountCommand = "account";
		internal const string ServiceAccountUsernameCommand = "username";
		internal const string ServiceAccountPasswordCommand = "password";

	    private readonly IRuntimeSettings _runtimeSettings;
	    
	    public WindowsServiceHandler(IRuntimeSettings runtimeSettings)
	    {
		    if (runtimeSettings == null) throw new ArgumentNullException("runtimeSettings");

		    _runtimeSettings = runtimeSettings;
	    }

        public bool Handle(HostArguments args, WindowsService service)
        {
            if (args == null) throw new ArgumentNullException("args");
	        if (service == null) throw new ArgumentNullException("service");

			string action;
			if (!args.CommandArgs.TryGetValue(ServiceCommand, out action))
                return false;

	        Func<KeyValuePair<string, string>, bool> actionIs = command =>
		        String.Equals(command.Value, action, StringComparison.OrdinalIgnoreCase);

            if (actionIs(InstallCommand))
            {
                using (var installer = new WindowsServiceInstaller(_runtimeSettings, service))
                {
	                string account;
	                args.CommandArgs.TryGetValue(ServiceAccountCommand, out account);

	                ServiceAccount serviceAccount;
	                if (Enum.TryParse(account, out serviceAccount))
		                installer.WithAccount(serviceAccount);

	                string username;
	                args.CommandArgs.TryGetValue(ServiceAccountUsernameCommand, out username);

	                string password;
	                args.CommandArgs.TryGetValue(ServiceAccountPasswordCommand, out password);

	                if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
		                installer.WithCredentials(username, password);

	                KeyValuePair<string, string>[] commandArgs = args.CommandArgs
		                .Where(x => !ReservedCommandArgs.Contains(x.Key, StringComparer.OrdinalIgnoreCase))
		                .Append(new KeyValuePair<string, string>("service", String.Empty))
		                .ToArray();

                    installer.Install(new HostArguments(args.Command, commandArgs, args.Args.ToArray()));
                }
            }
            else if (actionIs(UninstallCommand))
            {
	            using (var installer = new WindowsServiceInstaller(_runtimeSettings, service))
                {
                    installer.Uninstall();
                }
            }
			else
			{
				using (var runner = new WindowsServiceRunner(_runtimeSettings, service))
				{
					ServiceBase.Run(runner);
				}
			}

            return true;
        }

		private IEnumerable<string> ReservedCommandArgs
		{
			get
			{
				yield return ServiceCommand;
				yield return ServiceAccountCommand;
				yield return ServiceAccountUsernameCommand;
				yield return ServiceAccountPasswordCommand;
			}
		}

		public static KeyValuePair<string, string> InstallCommand
		{
			get { return new KeyValuePair<string, string>(ServiceCommand, "install"); }
		}

		public static KeyValuePair<string, string> UninstallCommand
		{
			get { return new KeyValuePair<string, string>(ServiceCommand, "uninstall"); }
		}
    }
}