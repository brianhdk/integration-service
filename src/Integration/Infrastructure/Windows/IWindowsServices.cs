using System;
using System.ServiceProcess;

namespace Vertica.Integration.Infrastructure.Windows
{
	public interface IWindowsServices : IDisposable
	{
		bool Exists(string serviceName);
		ServiceControllerStatus GetStatus(string serviceName);

		void Start(string serviceName, string[] args = null);
		void Stop(string serviceName);
		void Restart(string serviceName, string[] args = null);

		void Install(WindowsServiceConfiguration windowsService);
		void Uninstall(string serviceName);

		void Run(string serviceName, Func<IDisposable> onStartFactory);
	}
}