using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Castle.MicroKernel;
using Castle.Windsor;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Tracing;
using Microsoft.Owin;
using Owin;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.WebApi.Infrastructure;
using Vertica.Integration.WebApi.SignalR.Infrastructure.Castle.Windsor;
using IDependencyResolver = Microsoft.AspNet.SignalR.IDependencyResolver;

namespace Vertica.Integration.WebApi.SignalR
{
    public class SignalRConfiguration : IInitializable<IWindsorContainer>
    {
        private readonly List<Assembly> _assemblies;
	    private readonly List<Type> _removeHubs; 
		private readonly List<Type> _removeModules;

	    private bool _skipTraceConfiguration;
	    private SourceLevels _traceLevel;

	    private bool _enableDetailedErrors;
	    private bool _enableJavaScriptProxies;
	    private bool _enableJSONP;

        private PathString _path;
        private Action<IAppBuilder> _onMap;

        internal SignalRConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application;

            _assemblies = new List<Assembly>();
			_removeModules = new List<Type>();
			_removeHubs = new List<Type>();

		    AddFromAssemblyOfThis<SignalRConfiguration>();

			_traceLevel = SourceLevels.Warning;

		    _enableDetailedErrors = true;
		    _enableJavaScriptProxies = true;

            _path = new PathString("/signalr");

            // Register configuration
            Application.UseWebApi(webApi => webApi.HttpServer(httpServer => httpServer.Configure(Configure)));
        }

        public ApplicationConfiguration Application { get; }

		public SignalRConfiguration AddFromAssemblyOfThis<T>()
        {
            _assemblies.Add(typeof (T).Assembly);

            return this;
        }

	    public SignalRConfiguration RemoveHub<THub>() where THub : Hub
	    {
		    _removeHubs.Add(typeof (THub));

		    return this;
	    }

		public SignalRConfiguration RemovePipeline<THubPipelineModule>() where THubPipelineModule : HubPipelineModule
		{
			_removeModules.Add(typeof (THubPipelineModule));

			return this;
		}

	    public SignalRConfiguration DisableDetailedErrors()
	    {
		    _enableDetailedErrors = false;

		    return this;
	    }

	    public SignalRConfiguration DisableJavaScriptProxies()
	    {
		    _enableJavaScriptProxies = false;

		    return this;
	    }

	    public SignalRConfiguration EnableJSONP()
	    {
		    _enableJSONP = true;

		    return this;
	    }

        public SignalRConfiguration Path(PathString path)
        {
            _path = path;

            return this;
        }

        public SignalRConfiguration OnMap(Action<IAppBuilder> subApp)
        {
            if (subApp == null) throw new ArgumentNullException(nameof(subApp));

            _onMap = subApp;

            return this;
        }

		/// <summary>
		/// Call this method if you're using app.config / system.diagnostics to setup SignalR tracing.
		/// We'll make sure not to change any tracing configuration in SignalR, e.g. TraceLevel.
		/// http://www.asp.net/signalr/overview/testing-and-debugging/enabling-signalr-tracing
		/// </summary>
	    public SignalRConfiguration SkipTraceConfiguration()
	    {
		    _skipTraceConfiguration = true;
		    return this;
	    }

		/// <summary>
		/// By default we'll set the TraceLevel to 'Warning'. SignalR will use our TextWriter implementation to output information.
		/// </summary>
	    public SignalRConfiguration TraceLevel(SourceLevels level)
	    {
		    _traceLevel = level;
		    return this;
	    }

		void IInitializable<IWindsorContainer>.Initialized(IWindsorContainer container)
		{
			container.Install(new SignalRInstaller(_assemblies.ToArray(), _removeHubs.ToArray(), _removeModules.ToArray()));
		}

	    private void Configure(IOwinConfiguration owin)
	    {
		    if (owin == null) throw new ArgumentNullException(nameof(owin));

		    owin.Http.Routes.IgnoreRoute("signalR", "signalr/{*pathInfo}");

			IDependencyResolver resolver =
				GlobalHost.DependencyResolver =
					new CustomResolver(GlobalHost.DependencyResolver, owin.Kernel);

			var hubConfiguration = new HubConfiguration
			{
				Resolver = resolver,
				EnableDetailedErrors = _enableDetailedErrors,
				EnableJavaScriptProxies = _enableJavaScriptProxies,
				EnableJSONP = _enableJSONP
			};

		    resolver.Register(typeof (IAssemblyLocator), () => new CustomAssemblyLocator(_assemblies));
			resolver.Register(typeof (IHubActivator), () => new CustomHubActivator(resolver));
			resolver.Register(typeof (IHubDescriptorProvider), () => new CustomHubDescriptorProvider(resolver, owin.Kernel.Resolve<IHubsProvider>()));

			IHubPipeline hubPipeline = resolver.Resolve<IHubPipeline>();
			foreach (var pipelineModule in owin.Kernel.ResolveAll<IHubPipelineModule>())
				hubPipeline.AddModule(pipelineModule);

	        owin.App.Map(_path, subApp =>
	        {
                _onMap?.Invoke(subApp);

                subApp.RunSignalR(hubConfiguration);
	        });
            
			// TODO: Look at the possibility to add custom trace sources programatically
			// https://msdn.microsoft.com/en-us/library/ms228984(v=vs.110).aspx

		    if (!_skipTraceConfiguration)
		    {
				ITraceManager traceManager = resolver.Resolve<ITraceManager>();
				traceManager.Switch.Level = _traceLevel;			    
		    }
	    }

		/// <summary>
		/// Used by HubDescriptor to locate all Hubs.
		/// </summary>
		private class CustomAssemblyLocator : IAssemblyLocator
		{
			private readonly IList<Assembly> _assemblies;

			public CustomAssemblyLocator(IList<Assembly> assemblies)
			{
				_assemblies = assemblies;
			}

			public IList<Assembly> GetAssemblies()
			{
				return _assemblies;
			}
		}

		private class CustomHubDescriptorProvider : ReflectedHubDescriptorProvider, IHubDescriptorProvider
		{
			private readonly HashSet<Type> _hubs;

			public CustomHubDescriptorProvider(IDependencyResolver resolver, IHubsProvider hubsProvider)
				: base(resolver)
			{
				_hubs = new HashSet<Type>(hubsProvider.Hubs.Distinct());
			}

			public new IList<HubDescriptor> GetHubs()
			{
				return base.GetHubs().Where(x => _hubs.Contains(x.HubType)).ToList();
			}

			public new bool TryGetHub(string hubName, out HubDescriptor descriptor)
			{
				if (base.TryGetHub(hubName, out descriptor) && _hubs.Contains(descriptor.HubType))
					return true;

				return false;
			}
		}

		private class CustomResolver : IDependencyResolver
		{
			private readonly IDependencyResolver _defaultResolver;
			private readonly IKernel _kernel;

			public CustomResolver(IDependencyResolver defaultResolver, IKernel kernel)
			{
				if (defaultResolver == null) throw new ArgumentNullException(nameof(defaultResolver));
				if (kernel == null) throw new ArgumentNullException(nameof(kernel));

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
				if (resolver == null) throw new ArgumentNullException(nameof(resolver));

				_resolver = resolver;
			}

			public IHub Create(HubDescriptor descriptor)
			{
				if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));

				if (descriptor.HubType == null)
					return null;

				return _resolver.Resolve(descriptor.HubType) as IHub;
			}
		}
    }
}