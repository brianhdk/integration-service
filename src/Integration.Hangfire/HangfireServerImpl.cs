using System;
using System.Collections.Generic;
using Castle.MicroKernel;
using Hangfire;
using Hangfire.Server;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Hangfire
{
	internal class HangfireServerImpl : IDisposable
	{
		private readonly IKernel _kernel;
		private readonly IInternalConfiguration _configuration;

		private readonly BackgroundJobServer _server;

		public HangfireServerImpl(IKernel kernel, IInternalConfiguration configuration)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));

			_kernel = kernel;
			_configuration = configuration;

			Execute(_configuration.OnStartup);

		    JobActivator.Current = _configuration.ServerOptions.Activator = new WindsorJobActivator(kernel);

            IBackgroundProcess[] backgroundProcesses = kernel.ResolveAll<IBackgroundProcess>();

            _server = new BackgroundJobServer(configuration.ServerOptions, JobStorage.Current, backgroundProcesses);
		}

		public void Dispose()
		{
			_server.Dispose();

			Execute(_configuration.OnShutdown);
		}

		private void Execute(IEnumerable<Action<IKernel>> actions)
		{
			foreach (Action<IKernel> action in actions)
				action(_kernel);
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