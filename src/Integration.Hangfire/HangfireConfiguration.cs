using System;
using Castle.MicroKernel;
using Castle.Windsor;
using Hangfire;
using Hangfire.Server;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Hangfire
{
	public class HangfireConfiguration : IInitializable<IWindsorContainer>
	{
		private readonly IInternalConfiguration _configuration;
		private readonly ScanAddRemoveInstaller<IBackgroundProcess> _backgroundProcesses;
		
		internal HangfireConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application
				.Hosts(hosts => hosts.Host<HangfireHost>());

			_configuration = new InternalConfiguration();

			_backgroundProcesses = new ScanAddRemoveInstaller<IBackgroundProcess>();
		}

		public ApplicationConfiguration Application { get; }

		public HangfireConfiguration Configuration(Action<IGlobalConfiguration> configuration)
		{
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));

			configuration(GlobalConfiguration.Configuration);

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

		void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			JobActivator.Current = _configuration.ServerOptions.Activator = new WindsorJobActivator(container.Kernel);

			container.Install(_backgroundProcesses);
			container.Install(Install.Instance(_configuration));
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
	}
}