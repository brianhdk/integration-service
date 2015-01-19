using System;
using System.Net;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;

namespace Vertica.Integration
{
    public sealed class ApplicationContext : IDisposable
    {
        private static readonly Lazy<Action> EnsureSingleton = new Lazy<Action>(() => () => { });
 
        private readonly IWindsorContainer _container;

        private ApplicationContext(Action<ApplicationConfiguration> builder)
        {
            var configuration = new ApplicationConfiguration();

            if (builder != null)
                builder(configuration);

			ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            _container = CastleWindsor.Initialize(configuration);

			AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => LogException(eventArgs);
		}

	    public static ApplicationContext Create(Action<ApplicationConfiguration> builder = null)
	    {
            if (EnsureSingleton.IsValueCreated)
			    throw new InvalidOperationException("An instance of ApplicationContext has already been created.");

	        EnsureSingleton.Value();

	        return new ApplicationContext(builder);
	    }

	    public ITaskService TaskService
		{
			get { return _container.Resolve<ITaskService>(); }
		}

	    private void LogException(UnhandledExceptionEventArgs e)
	    {
	        var exception = e.ExceptionObject as Exception;

            if (exception == null)
                return;

            if (exception is TaskExecutionFailedException)
                return;

	        _container.Resolve<ILogger>().LogError(exception);
	    }

		public void Dispose()
		{
			_container.Dispose();
		}
	}
}