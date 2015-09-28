using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace Vertica.Integration.Infrastructure.Windows
{
	internal class WindowsServices : IWindowsServices
	{
		private readonly string _machineName;
		private readonly Lazy<ServiceController[]> _services; 

		public WindowsServices(string machineName = null)
		{
			_machineName = machineName;
			_services = new Lazy<ServiceController[]>(GetServices);
		}

		public bool Exists(string serviceName)
		{
			return GetService(serviceName, throwIfNotFound: false) != null;
		}

		public ServiceControllerStatus GetStatus(string serviceName)
		{
			ServiceController service = GetService(serviceName);

			return service.Status;
		}

		public void Start(string serviceName, string[] args = null)
		{
			Ensure(serviceName, ServiceControllerStatus.Running, service => service.Start(args ?? new string[0]));
		}

		public void Stop(string serviceName)
		{
			Ensure(serviceName, ServiceControllerStatus.Stopped, service => service.Stop());
		}

		public void Restart(string serviceName, string[] args = null)
		{
			Stop(serviceName);
			Start(serviceName, args);
		}

		public void Install(WindowsServiceConfiguration windowsService)
		{
			if (windowsService == null) throw new ArgumentNullException("windowsService");

			using (var process = new ServiceProcessInstaller())
			using (var installer = windowsService.CreateInstaller(process))
			{
				if (windowsService.Args != null)
				{
					installer.AfterInstall += (sender, installArgs) =>
					{
						ServiceController[] services = GetServices();

						ServiceController controller = services
							.SingleOrDefault(x => x.ServiceName.Equals(installer.ServiceName));

						if (controller != null)
							Win32Service.SetServiceArguments(controller, windowsService.ExePath, windowsService.Args);

						Dispose(services);
					};					
				}

				installer.Install(new Hashtable());
			}
		}

		public void Uninstall(string serviceName)
		{
			ServiceController service = GetService(serviceName);

			using (var process = new ServiceProcessInstaller())
			using (var installer = new ServiceInstaller())
			{
				installer.Context = new InstallContext(String.Empty, new string[0]);
				installer.ServiceName = service.ServiceName;
				installer.Parent = process;

				// ReSharper disable AssignNullToNotNullAttribute
				installer.Uninstall(null); // dictionary must be null, otherwise uninstall will fail
				// ReSharper restore AssignNullToNotNullAttribute
			}
		}

		public void Run(string serviceName, Func<IDisposable> onStartFactory)
		{
			if (String.IsNullOrWhiteSpace(serviceName)) throw new ArgumentException(@"Value cannot be null or empty.", "serviceName");

			using (var runner = new WindowsServiceRunner(serviceName, onStartFactory))
			{
				ServiceBase.Run(runner);
			}
		}

		public void Dispose()
		{
			if (!_services.IsValueCreated)
				return;

			Dispose(_services.Value);
		}

		private static void Dispose(ServiceController[] services)
		{
			foreach (var service in services)
				service.Dispose();
		}

		private void Ensure(string serviceName, ServiceControllerStatus status, Action<ServiceController> action)
		{
			ServiceController service = GetService(serviceName);

			if (service.Status != status)
			{
				action(service);
				service.WaitForStatus(status);
			}
		}

		// ReSharper disable once UnusedParameter.Local
		private ServiceController GetService(string serviceName, bool throwIfNotFound = true)
		{
			if (String.IsNullOrWhiteSpace(serviceName)) throw new ArgumentException(@"Value cannot be null or empty.", "serviceName");

			ServiceController service = _services.Value.SingleOrDefault(x =>
				String.Equals(x.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase));

			if (service == null && throwIfNotFound)
				throw new InvalidOperationException(
					String.Format("Service with name '{0}' does not exist.{1}", 
						serviceName,
						!String.IsNullOrWhiteSpace(_machineName) ? String.Format(" Machine: {0}", _machineName) : String.Empty));

			return service;
		}

		private ServiceController[] GetServices()
		{
			if (String.IsNullOrWhiteSpace(_machineName))
				return ServiceController.GetServices();

			return ServiceController.GetServices(_machineName);
		}

		private static class Win32Service
		{
			public static void SetServiceArguments(ServiceController serviceController, string exePath, string args)
			{
				exePath = Path.GetFullPath(exePath.Trim(' ', '\'', '"'));
				exePath = String.Format("\"{0}\" {1}", exePath, args).TrimEnd();

				if (!ChangeConfiguration(serviceController, exePath))
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