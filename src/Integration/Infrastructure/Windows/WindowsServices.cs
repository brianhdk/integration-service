﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Windows
{
	internal class WindowsServices : IWindowsServices
	{
		private readonly string _machineName;

		public WindowsServices(string machineName = null)
		{
			_machineName = machineName;
		}

		public bool Exists(string serviceName)
		{
			return WithService(serviceName, service => service != null, throwIfNotFound: false);
		}

		public ServiceControllerStatus GetStatus(string serviceName)
		{
			return WithService(serviceName, service => service.Status);
		}

		public void Start(string serviceName, string[] args = null, TimeSpan? timeout = null)
		{
			Ensure(serviceName, ServiceControllerStatus.Running, service => service.Start(args ?? new string[0]), timeout);
		}

		public void Stop(string serviceName, TimeSpan? timeout = null)
		{
			Ensure(serviceName, ServiceControllerStatus.Stopped, service => service.Stop(), timeout);
		}

		public void Install(WindowsServiceConfiguration windowsService)
		{
			if (windowsService == null) throw new ArgumentNullException("windowsService");

			if (!String.IsNullOrWhiteSpace(_machineName))
				throw new InvalidOperationException("Not supported on remote machines.");

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
			WithService(serviceName, service =>
			{
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

				return true;
			});
		}

		public void Run(string serviceName, Func<IDisposable> onStartFactory)
		{
			if (String.IsNullOrWhiteSpace(serviceName)) throw new ArgumentException(@"Value cannot be null or empty.", "serviceName");

			using (var runner = new WindowsServiceRunner(serviceName, onStartFactory))
			{
				ServiceBase.Run(runner);
			}
		}

		private static void Dispose(ServiceController[] services)
		{
			foreach (var service in services)
				service.Dispose();
		}

		private void Ensure(string serviceName, ServiceControllerStatus status, Action<ServiceController> action, TimeSpan? timeout)
		{
			WithService(serviceName, service =>
			{
				if (service.Status != status)
				{
					action(service);

					if (timeout.HasValue)
						service.WaitForStatus(status, timeout.Value);
					else
						service.WaitForStatus(status);
				}

				return new object();
			});
		}

		private TResult WithService<TResult>(string serviceName, Func<ServiceController, TResult> service, bool throwIfNotFound = true)
		{
			if (String.IsNullOrWhiteSpace(serviceName)) throw new ArgumentException(@"Value cannot be null or empty.", "serviceName");

			TResult result = default(TResult);

			using (GetServices(services =>
			{
				ServiceController actualService = services.SingleOrDefault(x =>
					String.Equals(x.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase));

				if (actualService == null && throwIfNotFound)
					throw new InvalidOperationException(
						String.Format("Service with name '{0}' does not exist.{1}",
							serviceName,
							!String.IsNullOrWhiteSpace(_machineName) ? String.Format(" Machine: {0}", _machineName) : String.Empty));

				result = service(actualService);
			}))
			{
				return result;
			}
		}

		private IDisposable GetServices(Action<ServiceController[]> services)
		{
			ServiceController[] list = GetServices();
			services(list);

			return new DisposableAction(() => Dispose(list));
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