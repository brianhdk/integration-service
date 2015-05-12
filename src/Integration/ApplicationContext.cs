using System;
using System.Linq;
using System.Net;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;
using Vertica.Integration.Model.Web;
using Vertica.Integration.Startup;

namespace Vertica.Integration
{
    public sealed class ApplicationContext : IDisposable
    {
        private static readonly Lazy<Action> EnsureSingleton = new Lazy<Action>(() => () => { });
 
        private readonly IWindsorContainer _container;
        private readonly StartupAction[] _starters;

        private ApplicationContext(Action<ApplicationConfiguration> builder)
        {
            var configuration = new ApplicationConfiguration();

            if (builder != null)
                builder(configuration);

            if (configuration.IgnoreSslErrors)
			    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            _container = ObjectFactory.Create(() => CastleWindsor.Initialize(configuration));

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => LogException(eventArgs);

            _starters = new StartupAction[]
            {
                new StartWebApiHost(_container),                    // StartWebApiTask -url http://localhost:8123
                new RunTask(_container),                            // WriteDocumentationTask
                new RunTaskFromWebServiceInConsole(_container),     // WriteDocumentationTask -url http://localhost:8123
                new RunTaskFromWindowsService(_container),          // WriteDocumentationTask -service [url|seconds]
                new InstallWindowsServiceTaskHost(_container),      // WriteDocumentationTask -install [url|seconds]
                new UninstallWindowsServiceTaskHost(_container)     // WriteDocumentationTask -uninstall
            };
		}

	    public static ApplicationContext Create(Action<ApplicationConfiguration> builder = null)
	    {
            if (EnsureSingleton.IsValueCreated)
			    throw new InvalidOperationException("An instance of ApplicationContext has already been created.");

	        EnsureSingleton.Value();

	        return new ApplicationContext(builder);
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

            starter.Execute(context);
        }

        public void Dispose()
		{
			_container.Dispose();
		}

        private void LogException(UnhandledExceptionEventArgs e)
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

            _container.Resolve<ILogger>().LogError(exception);
        }
    }
}