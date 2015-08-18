using System;
using System.Diagnostics;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Tracing;
using Owin;

namespace Vertica.Integration.WebApi.SignalR
{
	public static class SignalRExtensions
	{
		public static WebApiConfiguration WithSignalR(this WebApiConfiguration webApi, Action<SignalRConfiguration> signalR)
		{
			if (webApi == null) throw new ArgumentNullException("webApi");
			if (signalR == null) throw new ArgumentNullException("signalR");

			webApi.Application.Extensibility(extensibility =>
			{
				extensibility.Cache(() =>
				{
					var configuration = new SignalRConfiguration(webApi.Application);

					signalR(configuration);

					webApi.HttpServer(httpServer => httpServer.Configure(owin =>
					{
						owin.Http.Routes.IgnoreRoute("signalR", "signalr/{*pathInfo}");

						var hubConfiguration = new HubConfiguration
						{
							EnableDetailedErrors = true
						};

						owin.App.MapSignalR(hubConfiguration);

						IDependencyResolver resolver = hubConfiguration.Resolver;

						resolver.Register(typeof(IAssemblyLocator), () => owin.Kernel.Resolve<IAssemblyLocator>());

						ITraceManager traceManager = resolver.Resolve<ITraceManager>();
						traceManager.Switch.Level = SourceLevels.Warning;

						//IHubPipeline hubPipeline = resolver.Resolve<IHubPipeline>();
						//hubPipeline.AddModule(new MyErrorModule());

						//GlobalHost.TraceManager.Switch.Level = SourceLevels.Warning;
						//GlobalHost.HubPipeline.AddModule(new MyErrorModule());
					}));

					return configuration;
				});
			});

			return webApi;
		}
	}
}