using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Web.Http;
using System.Web.Http.Routing;
using Microsoft.Owin;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Windows;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;
using Vertica.Integration.WebApi;
using Vertica.Integration.WebApi.Infrastructure;
using Vertica.Integration.WebApi.SignalR;

namespace Vertica.Integration.Experiments.BizTalkTracker
{
	public static class BizTalkTrackerTester
	{
		public static ApplicationConfiguration TestBizTalkTracker(this ApplicationConfiguration application)
		{
			return application
				.Database(database => database.DisableIntegrationDb())
				.Hosts(hosts => hosts.Clear().Host<BizTalkTrackerTesterHost>())
				.UseWebApi(webApi => webApi
					.AddFromAssemblyOfThis<DummyTask>())
				.Tasks(tasks => tasks.Task<DummyTask>());
		}

		public class BizTalkTrackerTesterHost : IHost
		{
			private readonly IWindowsServices _windowsServices;
			private readonly IHttpServerFactory _httpServerFactory;
			private readonly TextWriter _textWriter;

			public BizTalkTrackerTesterHost(IWindowsFactory windowsFactory, IHttpServerFactory httpServerFactory, TextWriter textWriter)
			{
				_httpServerFactory = httpServerFactory;
				_textWriter = textWriter;
				_windowsServices = windowsFactory.WindowsServices();
			}

			public bool CanHandle(HostArguments args)
			{
				return true;
			}

			public void Handle(HostArguments args)
			{
				// Installer windows service hvis den ikke allerede eksisterer

				// Hvis vi kører i UDV - så start tasken her.

				string username = _textWriter.ReadLine("Enter username: ");
				string password = _textWriter.ReadPassword();

				_textWriter.WriteLine("{0} / {1}", username, password);

				//_windowsServices.Start("BizTalkTrackerHost");
				//Console.WriteLine("Started");

				//if (Environment.UserInteractive)
				//{
				//	if (!_windowsServices.Exists(this.Name()))
				//	{
				//		_windowsServices.Install(
				//			new WindowsServiceConfiguration(
				//				this.Name(),
				//				Assembly.GetEntryAssembly().Location)
				//				.Description(Description)
				//				.DisplayName(this.Name())
				//				.StartMode(ServiceStartMode.Automatic));
				//	}
				//	else if (args.CommandArgs.Contains("uninstall"))
				//	{
				//		_windowsServices.Uninstall(this.Name());
				//	}
				//	else
				//	{
				//		_windowsServices.Start(this.Name());
				//		Process.Start("http://localhost");
				//	}
				//}
				//else
				//{
				//	_windowsServices.Run(this.Name(), () =>
				//	{
				//		return TimeSpan.FromSeconds(5).Repeat(() =>
				//		{
				//			File.WriteAllText(@"c:\tmp\" + Guid.NewGuid().ToString("N") + ".txt", "");
				//		});
				//	});							
				//}

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