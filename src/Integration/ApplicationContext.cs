using System;
using System.Linq;
using System.Net;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories;
using Vertica.Integration.Infrastructure.Logging;
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

        private ApplicationContext(Action<ApplicationConfiguration> builder)
        {
            var configuration = new ApplicationConfiguration();

            if (builder != null)
                builder(configuration);

            if (configuration.IgnoreSslErrors)
			    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            _container = ObjectFactory.Create(() => CastleWindsor.Initialize(configuration));

            _starters = new StartupAction[]
            {
                new StartWebApiHost(_container),
                new RunTask(_container),
                new RunTaskFromWebServiceInConsole(_container),
                new RunTaskFromWindowsService(_container),
                new InstallWindowsServiceTaskHost(_container),
                new UninstallWindowsServiceTaskHost(_container)
            };

            // TODO: se efter om vi skal placere denne så tidligt som muligt - også før IoC er på plads (EventViewer)
			AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => LogException(eventArgs);
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

            StartupAction action = _starters.FirstOrDefault(x => x.IsSatisfiedBy(context));

            if (action == null)
                throw new StartupActionNotFoundException(context);

            action.Execute(context);
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

            // TODO: Investigate into logging to event-viewer if everything else fails.
            _container.Resolve<ILogger>().LogError(exception);
        }
    }
}