using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Windows;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Experiments.BizTalkTracker
{
	public static class BizTalkTrackerTester
	{
		public static ApplicationConfiguration TestBizTalkTracker(this ApplicationConfiguration application)
		{
			return application
				.Database(database => database.DisableIntegrationDb())
				//.Hosts(hosts => hosts.Clear().Host<BizTalkTrackerHost>())
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

				using (IWindowsServices windowsServices = _windowsFactory.CreateWindowsServices())
				{
					// Hvis vi kører i UDV - så start tasken her.

					if (Environment.UserInteractive)
					{
						if (!windowsServices.Exists(this.Name()))
						{
							windowsServices.Install(
								new WindowsServiceConfiguration(
									this.Name(),
									Assembly.GetEntryAssembly().Location)
									.Description(Description)
									.DisplayName(this.Name())
									.StartMode(ServiceStartMode.Automatic));
						}
						else if (args.CommandArgs.Contains("uninstall"))
						{
							windowsServices.Uninstall(this.Name());
						}
						else
						{
							windowsServices.Start(this.Name());
							Process.Start("http://localhost");
						}
					}
					else
					{
						windowsServices.Run(this.Name(), () =>
						{
							return TimeSpan.FromSeconds(5).Repeat(() =>
							{
								File.WriteAllText(@"c:\tmp\" + Guid.NewGuid().ToString("N") + ".txt", "");
							});
						});							
					}
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