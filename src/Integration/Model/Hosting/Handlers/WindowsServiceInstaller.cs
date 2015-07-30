using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text.RegularExpressions;

namespace Vertica.Integration.Model.Hosting.Handlers
{
    internal class WindowsServiceInstaller : IDisposable
    {
	    private readonly IRuntimeSettings _runtimeSettings;
	    private readonly WindowsService _service;
	    private readonly ServiceInstaller _installer;

        private Credentials _credentials;

        public WindowsServiceInstaller(IRuntimeSettings runtimeSettings, WindowsService service)
        {
	        if (runtimeSettings == null) throw new ArgumentNullException("runtimeSettings");
	        if (service == null) throw new ArgumentNullException("service");

	        _runtimeSettings = runtimeSettings;
	        _service = service;

	        _installer = new ServiceInstaller();
        }

        public WindowsServiceInstaller WithCredentials(string username, string password)
        {
            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException(@"Value cannot be null or empty.", "username");
            if (String.IsNullOrWhiteSpace(password)) throw new ArgumentException(@"Value cannot be null or empty.", "password");

            _credentials = new Credentials
            {
				Account = ServiceAccount.User,
                Username = username,
                Password = password
            };

            return this;
        }

	    public WindowsServiceInstaller WithAccount(ServiceAccount account)
	    {
		    _credentials = new Credentials
		    {
			    Account = account
		    };

		    return this;
	    }

        public void Install(HostArguments arguments)
        {
	        if (arguments == null) throw new ArgumentNullException("arguments");

	        using (var processInstaller = new ServiceProcessInstaller())
	        {
		        if (_credentials != null)
		        {
			        processInstaller.Account = _credentials.Account;
			        processInstaller.Username = _credentials.Username;
			        processInstaller.Password = _credentials.Password;
		        }

		        _installer.Context = GetInstallContext();
		        _installer.ServiceName = GetServiceName(_runtimeSettings, _service);
		        _installer.DisplayName = Prefix(_runtimeSettings, _service.Name);
		        _installer.Description = _service.Description;
		        _installer.StartType = ServiceStartMode.Manual;
		        _installer.Parent = processInstaller;

		        _installer.AfterInstall += (sender, args) =>
		        {
			        ServiceController controller =
				        ServiceController.GetServices()
					        .SingleOrDefault(x => x.ServiceName.Equals(_installer.ServiceName));

			        if (controller != null)
				        Win32Service.SetServiceArguments(controller, arguments);
		        };

		        _installer.Install(new Hashtable());
	        }
        }

        public void Uninstall()
        {
            using (var processInstaller = new ServiceProcessInstaller())
            {
                _installer.Context = GetInstallContext();
                _installer.ServiceName = GetServiceName(_runtimeSettings, _service);
                _installer.Parent = processInstaller;

                // ReSharper disable AssignNullToNotNullAttribute
                _installer.Uninstall(null); // dictionary must be null, otherwise uninstall will fail
                // ReSharper restore AssignNullToNotNullAttribute
            }
        }

        public void Dispose()
        {
            _installer.Dispose();
        }

	    public static string GetServiceName(IRuntimeSettings runtimeSettings, WindowsService service)
	    {
		    if (runtimeSettings == null) throw new ArgumentNullException("runtimeSettings");
		    if (service == null) throw new ArgumentNullException("service");

		    return Regex.Replace(Prefix(runtimeSettings, service.Name), @"\W", String.Empty);
	    }

	    private static string Prefix(IRuntimeSettings runtimeSettings, string value)
        {
	        ApplicationEnvironment environment = runtimeSettings.Environment;

	        return String.Format("Integration Service{0}: {1}", 
				environment != null
					? String.Format(" [{0}]", environment)
					: String.Empty, 
				value);
        }

        private static InstallContext GetInstallContext()
        {
            string[] commandLine =
            {
                String.Format("/assemblypath={0}", ExePath)
            };

            return new InstallContext(String.Empty, commandLine);
        }

        private static string ExePath
        {
            get { return Assembly.GetEntryAssembly().Location; }
        }

        private static class Win32Service
        {
            public static void SetServiceArguments(ServiceController serviceController, HostArguments arguments)
            {
                string exePath = Path.GetFullPath(ExePath.Trim(' ', '\'', '"'));

                string exePathWithArguments = 
					String.Format("\"{0}\" {1} {2} {3}", exePath, arguments.Command, arguments.CommandArgs, arguments.Args).TrimEnd();

                if (!ChangeConfiguration(serviceController, exePathWithArguments))
                    throw new Win32Exception();
            }

            private static bool ChangeConfiguration(ServiceController serviceController, string exePathWithArguments)
            {
                const int notChanged = -1;

                return
                    ChangeServiceConfig(serviceController.ServiceHandle, notChanged, notChanged, notChanged,
                        exePathWithArguments, null, IntPtr.Zero, null, null, null, null) != 0;
            }

            [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfigW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int ChangeServiceConfig(SafeHandle hService, int nServiceType, int nStartType,
                int nErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId,
                [In] string lpDependencies, string lpServiceStartName, string lpPassword, string lpDisplayName);
        }

        private class Credentials
        {
	        public ServiceAccount Account { get; set; }
	        public string Username { get; set; }
	        public string Password { get; set; }
        }
    }
}