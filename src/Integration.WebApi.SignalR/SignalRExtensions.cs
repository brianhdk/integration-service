using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using Castle.MicroKernel;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Tracing;
using Owin;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.WebApi.SignalR.Infrastructure;
using IDependencyResolver = Microsoft.AspNet.SignalR.IDependencyResolver;

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
				extensibility.Register(() =>
				{
					var configuration = new SignalRConfiguration(webApi.Application);

					signalR(configuration);

					webApi.HttpServer(httpServer => httpServer.Configure(owin =>
					{
						owin.Http.Routes.IgnoreRoute("signalR", "signalr/{*pathInfo}");

						GlobalHost.DependencyResolver = new CustomResolver(GlobalHost.DependencyResolver, owin.Kernel);

						var hubConfiguration = new HubConfiguration
						{
							EnableDetailedErrors = true
						};

						IDependencyResolver resolver = hubConfiguration.Resolver;
						resolver.Register(typeof (IAssemblyLocator), () => owin.Kernel.Resolve<IAssemblyLocator>());
						resolver.Register(typeof (IHubActivator), () => new CustomHubActivator(resolver));

						IHubPipeline hubPipeline = resolver.Resolve<IHubPipeline>();
						hubPipeline.AddModule(new ErrorLoggingModule(owin.Kernel.Resolve<ILogger>()));

						owin.App.MapSignalR(hubConfiguration);

						ITraceManager traceManager = resolver.Resolve<ITraceManager>();
						traceManager.Switch.Level = SourceLevels.Warning;
					}));

					return configuration;
				});
			});

			return webApi;
		}

		private class CustomResolver : IDependencyResolver
		{
			private readonly IDependencyResolver _defaultResolver;
			private readonly IKernel _kernel;

			public CustomResolver(IDependencyResolver defaultResolver, IKernel kernel)
			{
				if (defaultResolver == null) throw new ArgumentNullException("defaultResolver");
				if (kernel == null) throw new ArgumentNullException("kernel");

				_defaultResolver = defaultResolver;
				_kernel = kernel;
			}

			public object GetService(Type serviceType)
			{
				object service = _defaultResolver.GetService(serviceType);

				if (service == null && _kernel.HasComponent(serviceType))
				{
					try
					{
						service = _kernel.Resolve(serviceType);
					}
					catch (Exception ex)
					{
						_kernel.Resolve<ILogger>().LogError(ex);
						throw;
					}
				}

				return service;
			}

			public IEnumerable<object> GetServices(Type serviceType)
			{
				IEnumerable<object> services = _defaultResolver.GetServices(serviceType);

				if (services == null && _kernel.HasComponent(serviceType))
					return _kernel.ResolveAll(serviceType).OfType<object>();

				return services;
			}

			public void Register(Type serviceType, Func<object> activator)
			{
				_defaultResolver.Register(serviceType, activator);
			}

			public void Register(Type serviceType, IEnumerable<Func<object>> activators)
			{
				_defaultResolver.Register(serviceType, activators);
			}

			public void Dispose()
			{
				_defaultResolver.Dispose();
			}
		}

		private class CustomHubActivator : IHubActivator
		{
			private readonly IDependencyResolver _resolver;

			public CustomHubActivator(IDependencyResolver resolver)
			{
				if (resolver == null) throw new ArgumentNullException("resolver");

				_resolver = resolver;
			}

			public IHub Create(HubDescriptor descriptor)
			{
				if (descriptor == null) throw new ArgumentNullException("descriptor");

				if (descriptor.HubType == null)
					return null;

				return _resolver.Resolve(descriptor.HubType) as IHub;
			}
		}
	}
}