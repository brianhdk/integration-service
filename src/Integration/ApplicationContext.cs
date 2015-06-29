using System;
using System.Linq;
using System.Net;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Logging.Loggers;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;
using Vertica.Integration.Startup;

namespace Vertica.Integration
{
    public sealed class ApplicationContext : IDisposable
    {
        private static readonly Lazy<Action> EnsureSingleton = new Lazy<Action>(() => () => { });

        private readonly IWindsorContainer _container;
        private readonly StartupAction[] _starters;

        private ApplicationContext(Action<ApplicationConfiguration> application)
        {
            var configuration = new ApplicationConfiguration();

            if (application != null)
                application(configuration);

            if (configuration.IgnoreSslErrors)
			    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            _container = CastleWindsor.Initialize(configuration);

            AppDomain.CurrentDomain.UnhandledException += LogException;

            // TODO: Rethink this, consider making it open for extensibility
            _starters = new StartupAction[]
            {
                new StartWebApiHost(_container),                    // StartWebApiTask -url http://localhost:8123
                new RunTask(_container),                            // WriteDocumentationTask
                new HostTaskAsWebService(_container),               // WriteDocumentationTask -url http://localhost:8123
                new RunTaskFromWindowsService(_container),          // WriteDocumentationTask -service [url|seconds]
                new InstallWindowsServiceTaskHost(_container),      // WriteDocumentationTask -install [url|seconds]
                new UninstallWindowsServiceTaskHost(_container)     // WriteDocumentationTask -uninstall
            };
		}

	    public static ApplicationContext Create(Action<ApplicationConfiguration> application = null)
	    {
            if (EnsureSingleton.IsValueCreated)
			    throw new InvalidOperationException("An instance of ApplicationContext has already been created. It might have been disposed, but you should make sure to reuse the same instance for the entire lifecycle of this application.");

	        EnsureSingleton.Value();

	        return new ApplicationContext(application);
	    }

        public void Execute(params string[] args)
        {
            if (args == null) throw new ArgumentNullException("args");
            if (args.Length == 0) throw new ArgumentOutOfRangeException("args", @"No task name passed as argument.");

            // TODO: Overvej async.

            ITask task = _container.Resolve<ITaskFactory>().GetByName(args.First());

            var context = new ExecutionContext(task, args.Skip(1).ToArray());

            StartupAction starter = _starters.FirstOrDefault(x => x.IsSatisfiedBy(context));

            if (starter == null)
                throw new StartupActionNotFoundException(context);

            // TODO: Giv mulighed for at skifte ud - Console
            //  - måske med mulighed for at loggeren kan skiftes tilbage igen

            starter.Execute(context);
        }

        public void Dispose()
		{
			_container.Dispose();

            AppDomain.CurrentDomain.UnhandledException -= LogException;
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