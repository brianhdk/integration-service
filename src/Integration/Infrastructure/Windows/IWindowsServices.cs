using System;
using System.ServiceProcess;
using Vertica.Integration.Model.Hosting.Handlers;

namespace Vertica.Integration.Infrastructure.Windows
{
	public interface IWindowsServices
	{
		bool Exists(string serviceName);
		ServiceControllerStatus GetStatus(string serviceName);

		void Start(string serviceName, string[] args = null, TimeSpan? timeout = null);
		void Stop(string serviceName, TimeSpan? timeout = null);

		void Install(WindowsServiceConfiguration windowsService);
		void Uninstall(string serviceName);

        [Obsolete("No longer supported. Use '" + nameof(IWindowsServiceHandler) + "'.")]
		void Run(string serviceName, Func<IDisposable> onStartFactory);
	}
}