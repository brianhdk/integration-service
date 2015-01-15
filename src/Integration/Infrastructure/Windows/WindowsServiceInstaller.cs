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
using Vertica.Integration.Properties;

namespace Vertica.Integration.Infrastructure.Windows
{
    public class WindowsServiceInstaller : IDisposable
    {
        private readonly string _name;
        private readonly string _displayName;
        private readonly ServiceInstaller _installer;

        public WindowsServiceInstaller(string name, string displayName)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "displayName");
            if (String.IsNullOrWhiteSpace(displayName)) throw new ArgumentException(@"Value cannot be null or empty.", "displayName");

            _name = name;
            _displayName = displayName;

            _installer = new ServiceInstaller();
        }

        public void Install(string description, string[] arguments)
        {
            using (var processInstaller = new ServiceProcessInstaller())
            {
                if (!String.IsNullOrWhiteSpace(Settings.Default.WindowsServiceUsername))
                {
                    processInstaller.Account = ServiceAccount.User;
                    processInstaller.Username = Settings.Default.WindowsServiceUsername;
                    processInstaller.Password = Settings.Default.WindowsServicePassword;
                }

                _installer.Context = GetInstallContext();
                _installer.ServiceName = GetServiceName(_name);
                _installer.DisplayName = PrefixEnvironment(_displayName);
                _installer.Description = description;
                _installer.StartType = ServiceStartMode.Manual;
                _installer.Parent = processInstaller;

                if (arguments != null && arguments.Length > 0)
                {
                    _installer.AfterInstall += (sender, args) =>
                    {
                        ServiceController controller =
                            ServiceController.GetServices()
                                .SingleOrDefault(x => x.ServiceName.Equals(_installer.ServiceName));

                        if (controller != null)
                            Win32Service.SetServiceArguments(controller, arguments);
                    };
                }

                _installer.Install(new Hashtable());
            }
        }

        public void Uninstall()
        {
            using (var processInstaller = new ServiceProcessInstaller())
            {
                _installer.Context = GetInstallContext();
                _installer.ServiceName = GetServiceName(_name);
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

        public static string GetServiceName(string name)
        {
            return Regex.Replace(PrefixEnvironment(name), @"\W", String.Empty);
        }

        private static string PrefixEnvironment(string value)
        {
            return String.Format("{0}: {1}", Settings.Default.WindowsServiceNamePrefix, value);
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
            public static void SetServiceArguments(ServiceController serviceController, string[] arguments)
            {
                string exePath = Path.GetFullPath(ExePath.Trim(' ', '\'', '"'));

                string exePathWithArguments = String.Format("\"{0}\" {1}", exePath, String.Join(" ", arguments ?? new string[0])).TrimEnd();

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
    }
}