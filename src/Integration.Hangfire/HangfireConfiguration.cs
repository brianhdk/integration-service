using System;
using System.Collections.Generic;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Hangfire;
using Hangfire.Server;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Hangfire.Console;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Hangfire
{
	public class HangfireConfiguration : IInitializable<IWindsorContainer>
	{
		private readonly InternalConfiguration _configuration;
		private readonly ScanAddRemoveInstaller<IBackgroundProcess> _backgroundProcesses;
        private readonly List<Action<IGlobalConfiguration, IKernel>> _globalConfigurations;
		
		internal HangfireConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application
				.Hosts(hosts => hosts.Host<HangfireHost>());

			_configuration = new InternalConfiguration();
			_backgroundProcesses = new ScanAddRemoveInstaller<IBackgroundProcess>();
            _globalConfigurations = new List<Action<IGlobalConfiguration, IKernel>>();
		}

		public ApplicationConfiguration Application { get; }

		public HangfireConfiguration Configuration(Action<IGlobalConfiguration> configuration)
		{
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            return Configuration((innerConfiguration, kernel) => 
            {
                configuration(innerConfiguration);
            });
		}

	    public HangfireConfiguration Configuration(Action<IGlobalConfiguration, IKernel> configuration)
	    {
	        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

	        _globalConfigurations.Add(configuration);

	        return this;
	    }

	    public HangfireConfiguration EnableConsole(Action<HangfireConsoleConfiguration> console = null)
	    {
            Application.Extensibility(extensibility =>
            {
                var configuration = extensibility.Register(() => new HangfireConsoleConfiguration(this));

                console?.Invoke(configuration);
            });

            return this;
	    }

		public HangfireConfiguration WithServerOptions(Action<BackgroundJobServerOptions> serverOptions)
		{
			if (serverOptions == null) throw new ArgumentNullException(nameof(serverOptions));

			serverOptions(_configuration.ServerOptions);

			return this;
		}

		public HangfireConfiguration OnStartup(Action<StartupActions> startup)
		{
			if (startup == null) throw new ArgumentNullException(nameof(startup));

			startup(_configuration.OnStartup);

			return this;
		}
		
		public HangfireConfiguration OnShutdown(Action<ShutdownActions> shutdown)
		{
			if (shutdown == null) throw new ArgumentNullException(nameof(shutdown));

			shutdown(_configuration.OnShutdown);

			return this;
		}

		/// <summary>
		/// Scans the assembly of the defined <typeparamref name="T"></typeparamref> for public classes inheriting <see cref="IBackgroundProcess"/>.
		/// <para />
		/// </summary>
		/// <typeparam name="T">The type in which its assembly will be scanned.</typeparam>
		public HangfireConfiguration AddFromAssemblyOfThis<T>()
		{
			_backgroundProcesses.AddFromAssemblyOfThis<T>();

			return this;
		}

		/// <summary>
		/// Adds Hangfire to <see cref="ILiteServerFactory"/> allowing Hangfire to run simultaneously with other servers.
		/// </summary>
		public HangfireConfiguration AddToLiteServer()
		{
			Application.UseLiteServer(server => server.AddServer<HangfireBackgroundServer>());

			return this;
		}

		/// <summary>
		/// Adds the specified <typeparamref name="TProcess"/>.
		/// </summary>
		/// <typeparam name="TProcess">Specifies the <see cref="IBackgroundProcess"/> to be added.</typeparam>
		public HangfireConfiguration Add<TProcess>()
			where TProcess : IBackgroundProcess
		{
			_backgroundProcesses.Add<TProcess>();

			return this;
		}

		/// <summary>
		/// Skips the specified <typeparamref name="TProcess" />.
		/// </summary>
		/// <typeparam name="TProcess">Specifies the <see cref="IBackgroundProcess"/> that will be skipped.</typeparam>
		public HangfireConfiguration Remove<TProcess>()
			where TProcess : IBackgroundProcess
		{
			_backgroundProcesses.Remove<TProcess>();

			return this;
		}

		/// <summary>
		/// Clears all registred BackgroundProcesses.
		/// </summary>
		public HangfireConfiguration Clear()
		{
			_backgroundProcesses.Clear();

			return this;
		}

		void IInitializable<IWindsorContainer>.Initialized(IWindsorContainer container)
		{
            foreach (Action<IGlobalConfiguration, IKernel> globalConfiguration in _globalConfigurations)
                globalConfiguration(GlobalConfiguration.Configuration, container.Kernel);

            JobActivator.Current = _configuration.ServerOptions.Activator = new WindsorJobActivator(container.Kernel);

            container.Install(_backgroundProcesses);

			container.Register(
				Component.For<IHangfireServerFactory>()
					.UsingFactoryMethod(kernel => new HangfireServerFactory(kernel, _configuration)));
		}

		private class HangfireServerFactory : IHangfireServerFactory
		{
			private readonly IKernel _kernel;
			private readonly IInternalConfiguration _configuration;

			public HangfireServerFactory(IKernel kernel, IInternalConfiguration configuration)
			{
				_kernel = kernel;
				_configuration = configuration;
			}

			public IDisposable Create()
			{
				return new HangfireServerImpl(_kernel, _configuration);
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