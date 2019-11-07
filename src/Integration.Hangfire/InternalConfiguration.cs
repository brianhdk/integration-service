using System;
using System.Collections.Generic;
using Castle.MicroKernel;
using Hangfire;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Hangfire
{
	internal class InternalConfiguration
	{
        private readonly List<Action<IGlobalConfiguration, IKernel>> _configurations;
        private readonly List<Action<BackgroundJobServerOptions, IKernel>> _serverOptions;

        public InternalConfiguration()
		{
			OnStartup = new StartupActions();
			OnShutdown = new ShutdownActions();
            
		    _configurations = new List<Action<IGlobalConfiguration, IKernel>>();
            _serverOptions = new List<Action<BackgroundJobServerOptions, IKernel>>();
            ServerOptions = new BackgroundJobServerOptions();
        }

		public StartupActions OnStartup { get; }
		public ShutdownActions OnShutdown { get; }

        public IEnumerable<Action<IGlobalConfiguration, IKernel>> Configurations => _configurations;
        public BackgroundJobServerOptions ServerOptions { get; }

        public void ApplyServerOptions(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            foreach (Action<BackgroundJobServerOptions, IKernel> configurer in _serverOptions)
                configurer(ServerOptions, kernel);

            ServerOptions.Activator = new WindsorJobActivator(kernel);
        }
        
	    public void Add(Action<IGlobalConfiguration, IKernel> configuration)
	    {
	        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

	        _configurations.Add(configuration);
	    }

        public void Add(Action<BackgroundJobServerOptions, IKernel> serverOptions)
        {
            if (serverOptions == null) throw new ArgumentNullException(nameof(serverOptions));

            _serverOptions.Add(serverOptions);
        }

	    private class WindsorJobActivator : JobActivator
	    {
	        private readonly IKernel _kernel;

	        public WindsorJobActivator(IKernel kernel)
	        {
	            _kernel = kernel;
	        }

	        public override object ActivateJob(Type jobType)
	        {
	            try
	            {
	                return _kernel.Resolve(jobType);
	            }
	            catch (Exception ex)
	            {
	                _kernel.Resolve<ILogger>().LogError(ex);

	                throw;
	            }
	        }
	    }
    }
}