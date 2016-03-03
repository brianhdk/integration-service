using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.MicroKernel;
using Castle.Windsor;
using Hangfire;
using Hangfire.Server;
using Vertica.Integration.Hangfire.Infrastructure.Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Hangfire
{
	public class HangfireConfiguration : IInitializable<IWindsorContainer>
	{
		private readonly IInternalConfiguration _configuration;

		private readonly List<Assembly> _scan;
		private readonly List<Type> _add;
		private readonly List<Type> _remove;

		internal HangfireConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application
				.Hosts(hosts => hosts.Host<HangfireHost>());

			_configuration = new InternalConfiguration();

			_scan = new List<Assembly>();
			_add = new List<Type>();
			_remove = new List<Type>();
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
			_scan.Add(typeof (T).Assembly);

			return this;
		}

		/// <summary>
		/// Adds the specified <typeparamref name="TProcess"/>.
		/// </summary>
		/// <typeparam name="TProcess">Specifies the <see cref="IBackgroundProcess"/> to be added.</typeparam>
		public HangfireConfiguration Add<TProcess>()
			where TProcess : IBackgroundProcess
		{
			_add.Add(typeof(TProcess));

			return this;
		}

		/// <summary>
		/// Skips the specified <typeparamref name="TController" />.
		/// </summary>
		/// <typeparam name="TController">Specifies the <see cref="IBackgroundProcess"/> that will be skipped.</typeparam>
		public HangfireConfiguration Remove<TController>()
			where TController : IBackgroundProcess
		{
			_remove.Add(typeof (TController));

			return this;
		}

		/// <summary>
		/// Clears all registred BackgroundProcesses.
		/// </summary>
		public HangfireConfiguration Clear()
		{
			_remove.Clear();
			_add.Clear();
			_scan.Clear();

			return this;
		}

		void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			JobActivator.Current = _configuration.ServerOptions.Activator = new WindsorJobActivator(container.Kernel);

			container.Install(new HangfireInstaller(_scan.ToArray(), _add.ToArray(), _remove.ToArray()));
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