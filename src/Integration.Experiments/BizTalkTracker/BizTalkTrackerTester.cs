using Vertica.Integration.Infrastructure.Windows;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Experiments.BizTalkTracker
{
	public static class BizTalkTrackerTester
	{
		public static ApplicationConfiguration TestBizTalkTracker(this ApplicationConfiguration application)
		{
			return application
				.Database(database => database.DisableIntegrationDb())
				.Hosts(hosts => hosts.Clear().Host<BizTalkTrackerHost>())
				.Tasks(tasks => tasks.Task<DummyTask>());
		}

		public class BizTalkTrackerHost : IHost
		{
			private readonly IWindowsFactory _windowsFactory;

			public BizTalkTrackerHost(IWindowsFactory windowsFactory)
			{
				_windowsFactory = windowsFactory;
			}

			public bool CanHandle(HostArguments args)
			{
				return true;
			}

			public void Handle(HostArguments args)
			{
				// Installer windows service hvis den ikke allerede eksisterer

				using (IWindowsServices windowsServices = _windowsFactory.WindowsServices())
				{
					// For at vide om den allerede findes som Windows Service, bør dette tjekkes

					//var serviceExists = windowsServices.Exists("SomeNoneExistingService");
					//var status = windowsServices.GetStatus("hMailServer");
					//windowsServices.Restart("hMailServer");

					windowsServices.Uninstall("Integration.Host");

					windowsServices.Install(
						configuration => { },
						@"D:\vertica-tfs01\AMO-Toys\DigitalServicePlatform\development\src\Integration.Host\bin\Debug\Integration.Host.exe");
				}
			}

			public string Description
			{
				get { return "TBD"; }
			}
		}

		public class DummyTask : Task
		{
			public override string Description
			{
				get { return "TBD"; }
			}

			public override void StartTask(ITaskExecutionContext context)
			{

			}
		}
	}
}