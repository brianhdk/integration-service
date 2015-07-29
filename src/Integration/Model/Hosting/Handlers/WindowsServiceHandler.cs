using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class WindowsServiceHandler : IWindowsServiceHandler
    {
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
            if (!args.CommandArgs.TryGetValue("service", out action))
                return false;

	        Func<string, bool> actionIs = name =>
		        String.Equals(name, action, StringComparison.OrdinalIgnoreCase);

            if (actionIs("install"))
            {
                using (var installer = new WindowsServiceInstaller(_runtimeSettings, service))
                {
	                string account;
	                args.CommandArgs.TryGetValue("account", out account);

	                ServiceAccount serviceAccount;
	                if (Enum.TryParse(account, out serviceAccount))
		                installer.WithAccount(serviceAccount);

	                string username;
	                args.CommandArgs.TryGetValue("username", out username);

	                string password;
	                args.CommandArgs.TryGetValue("password", out password);

	                if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
		                installer.WithCredentials(username, password);

	                var commandArgs = new[]
	                {
		                new KeyValuePair<string, string>("service", String.Empty) 
	                };

                    installer.Install(new HostArguments(args.Command, commandArgs, args.Args.ToArray()));
                }
            }
            else if (actionIs("uninstall"))
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
    }
}