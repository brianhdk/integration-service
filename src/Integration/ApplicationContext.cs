using System;
using System.Linq;
using System.Net;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Logging.Loggers;
using Vertica.Integration.Model.Exceptions;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration
{
    public sealed class ApplicationContext : IApplicationContext
    {
        private static readonly Lazy<Action> EnsureSingleton = new Lazy<Action>(() => () => { });
		private static readonly Lazy<Action> Disposed = new Lazy<Action>(() => () => { });

	    private readonly ApplicationConfiguration _configuration;

	    private readonly IWindsorContainer _container;
	    private readonly IArgumentsParser _parser;
	    private readonly IHost[] _hosts;

	    private ApplicationContext(Action<ApplicationConfiguration> application)
        {
            _configuration = new ApplicationConfiguration();

            if (application != null)
                application(_configuration);

            if (_configuration.IgnoreSslErrors)
			    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            _container = CastleWindsor.Initialize(_configuration);
            _parser = _container.Resolve<IArgumentsParser>();
            _hosts = _container.Resolve<IHostFactory>().GetAll();

            AppDomain.CurrentDomain.UnhandledException += LogException;
		}

	    public static IApplicationContext Create(Action<ApplicationConfiguration> application = null)
	    {
            if (EnsureSingleton.IsValueCreated)
			    throw new InvalidOperationException("An instance of ApplicationContext has already been created. It might have been disposed, but you should make sure to reuse the same instance for the entire lifecycle of this application.");

	        EnsureSingleton.Value();

	        return new ApplicationContext(application);
	    }

        public void Execute(params string[] args)
        {
            if (args == null) throw new ArgumentNullException("args");

            Execute(_parser.Parse(args));
        }

        public void Execute(HostArguments args)
        {
            if (args == null) throw new ArgumentNullException("args");

            IHost[] hosts = _hosts.Where(x => x.CanHandle(args)).ToArray();

	        if (hosts.Length == 0)
		        throw new NoHostFoundException(args);

	        if (hosts.Length > 1)
		        throw new MultipleHostsFoundException(args, hosts);

            hosts[0].Handle(args);
        }

        public void Dispose()
		{
			if (Disposed.IsValueCreated)
				throw new InvalidOperationException("ApplicationContext has already been disposed.");

			Disposed.Value();

			AppDomain.CurrentDomain.UnhandledException -= LogException;

			_container.Dispose();
			_configuration.Dispose();
		}

        private void LogException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            if (exception == null)
                return;

            LogException(exception);
        }

        private void LogException(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");

            if (exception is TaskExecutionFailedException)
                return;

            var logger = _container.Resolve<ILogger>();

            try
            {
                logger.LogError(exception);
            }
            catch
            {
                if (logger is EventLogger)
                    throw;

                new EventLogger().LogError(exception);
            }
        }
    }
}