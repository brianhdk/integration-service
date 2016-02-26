using System;
using System.Collections.Generic;
using System.IO;
using Castle.MicroKernel;
using Hangfire;
using Hangfire.Server;
using Integration.Hangfire.Services;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model.Hosting;
using Vertica.Integration.Model.Hosting.Handlers;
using Vertica.Integration.WebApi.Infrastructure;

namespace Integration.Hangfire
{
	public class HangfireHost : IHost
	{
		internal static readonly string Command = typeof(HangfireHost).HostName();

		private readonly IWindowsServiceHandler _windowsService;
		private readonly TextWriter _outputter;
		private readonly IKernel _kernel;
		private readonly IHttpServerFactory _httpServerFactory;

		public HangfireHost(IWindowsServiceHandler windowsService, TextWriter outputter, IKernel kernel, IHttpServerFactory httpServerFactory)
		{
			_windowsService = windowsService;
			_outputter = outputter;
			_kernel = kernel;
			_httpServerFactory = httpServerFactory;
		}

		public bool CanHandle(HostArguments args)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));

			return string.Equals(args.Command, Command, StringComparison.OrdinalIgnoreCase);
		}

		public void Handle(HostArguments args)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));

			JobActivator.Current = new WindsorJobActivator(_kernel);

			Func<IDisposable> factory = Initialize;

			if (InstallOrRunAsWindowsService(args, factory))
				return;

			RecurringJob.AddOrUpdate<TaskByNameRunner>(x => x.Run(typeof(HangfireTask).TaskName(), "Full"), "*/1 * * * *");

			using (factory())
			{
				_outputter.WaitUntilEscapeKeyIsHit(@"Press ESCAPE to stop Hangfire...");
			}
		}

		private IDisposable Initialize()
		{
			BackgroundJobServerOptions options = new BackgroundJobServerOptions();
			JobStorage storage = JobStorage.Current;
			IEnumerable<IBackgroundProcess> processes = null; // resolve automatically

			//var hangfire = new BackgroundJobServer(options, storage, processes);

			var hangfireServer = new BackgroundJobServer();

			return new CompositeDisposable()
				.Add(hangfireServer)
				.Add(_httpServerFactory.Create("http://localhost:7654"));
		}

		private class CompositeDisposable : IDisposable
		{
			private readonly List<IDisposable> _disposables;

			public CompositeDisposable()
			{
				_disposables = new List<IDisposable>();
			}

			public CompositeDisposable Add(IDisposable disposable)
			{
				if (disposable == null) throw new ArgumentNullException(nameof(disposable));

				_disposables.Add(disposable);

				return this;
			}

			public void Dispose()
			{
				var exceptions = new List<Exception>();

				foreach (IDisposable disposable in _disposables)
				{
					try
					{
						disposable.Dispose();
					}
					catch (Exception ex)
					{
						exceptions.Add(ex);
					}
				}

				if (exceptions.Count > 0)
					throw new AggregateException(exceptions);
			}
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
				return _kernel.Resolve(jobType);
			}
		}

		private bool InstallOrRunAsWindowsService(HostArguments args, Func<IDisposable> factory)
		{
			return _windowsService.Handle(args, new HandleAsWindowsService(this.Name(), this.Name(), Description, factory));
		}

		public string Description => "Hangfire host.";
	}
}