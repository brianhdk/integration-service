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
	    private static readonly Lazy<ApplicationContext> Instance =
	        new Lazy<ApplicationContext>(() => new ApplicationContext());

		private readonly IWindsorContainer _windsorContainer;

	    private ApplicationContext()
		{
			ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

			_windsorContainer = Bootstrapper.Run();

			AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => LogException(eventArgs);
		}

	    public static ApplicationContext Create()
	    {
		    if (Instance.IsValueCreated)
			    throw new InvalidOperationException("An instance of ApplicationContext has already been created.");

			return Instance.Value;
		}

	    public ITaskService TaskService
		{
			get { return _windsorContainer.Resolve<ITaskService>(); }
		}

	    private void LogException(UnhandledExceptionEventArgs e)
	    {
	        var exception = e.ExceptionObject as Exception;

            if (exception == null)
                return;

            if (exception is TaskExecutionFailedException)
                return;

	        _windsorContainer.Resolve<ILogger>().LogError(exception);
	    }

		public void Dispose()
		{
			_windsorContainer.Dispose();
		}
	}
}