using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.Windsor;
using Hangfire;
using Hangfire.Server;
using Integration.Hangfire.Infrastructure.Castle.Windsor;
using Integration.Hangfire.Services;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.WebApi;
using Vertica.Integration.WebApi.Infrastructure;

namespace Integration.Hangfire
{
    public class HangfireConfiguration : IInitializable<IWindsorContainer>
    {
        private readonly List<Assembly> _scan;
        private readonly List<Type> _add;
        private readonly List<Type> _remove;

        internal HangfireConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application
				.Hosts(hosts => hosts.Host<HangfireHost>())
				.UseWebApi(webApi => webApi.HttpServer(httpServer => httpServer.Configure(HttpServerConfiguration)))
				.AddCustomInstaller(Install.Service<TaskByNameRunner>());

            _scan = new List<Assembly>();
            _add = new List<Type>();
            _remove = new List<Type>();
        }

	    private void HttpServerConfiguration(IOwinConfiguration configuration)
	    {
			// TODO: Combine WebApiHost WITH Hangfire
				// WebApiHost - start BackgroundServer
				// HangfireHost - start BackgroundServer + web-server

			// TODO: Configurable whether to run as web-server or directly
		    //configuration.App.UseHangfireServer();

			// issue is with the URL used for Hangfire

			if (!DashboardDisabled)
				configuration.App.UseHangfireDashboard();
	    }

		public bool DashboardDisabled { get; private set; }

		public HangfireConfiguration DisableDashboard()
		{
			DashboardDisabled = true;
			return this;
		}

	    public ApplicationConfiguration Application { get; private set; }

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
			container.Install(new HangfireInstaller(_scan.ToArray(), _add.ToArray(), _remove.ToArray()));
	    }
    }
}