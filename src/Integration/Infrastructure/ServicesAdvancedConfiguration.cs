using System;
using System.Collections.Generic;
using System.IO;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Features;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Logging.Loggers;
using Vertica.Integration.Infrastructure.Threading.DistributedMutex;
using Vertica.Integration.Infrastructure.Threading.DistributedMutex.Db;
using Vertica.Utilities.Extensions.EnumerableExt;
using Installer = Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers.Install;

namespace Vertica.Integration.Infrastructure
{
    public class ServicesAdvancedConfiguration : IInitializable<IWindsorContainer>
    {
	    private readonly IDictionary<Type, Registration> _registrations;
        private readonly List<IWindsorInstaller> _installers;

	    internal ServicesAdvancedConfiguration(ServicesConfiguration services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

	        _registrations = new Dictionary<Type, Registration>();
            _installers = new List<IWindsorInstaller>();

            Register<ILogger, DbLogger, EventLogger>();
		    Register<IConfigurationRepository, DbConfigurationRepository, FileBasedConfigurationRepository>();
		    Register<IArchiveService, DbArchiveService, FileBasedArchiveService>();
	        Register<IDistributedMutex, DbDistributedMutex, ThrowingDistributedMutex>();
		    Register<IRuntimeSettings, AppConfigRuntimeSettings>();
            Register<IFeatureToggler, InMemoryFeatureToggler>();

		    Register(kernel => Environment.UserInteractive ? Console.Out : TextWriter.Null);

            if (AzureWebJobShutdownRequest.IsRunningInAzure())
            {
                Register<IWaitForShutdownRequest, AzureWebJobShutdownRequest>();
            }
            else
            {
                Register<IWaitForShutdownRequest, WaitForEscapeKey>();
            }

            Services = services;
        }

        public ServicesConfiguration Services { get; }

        public ServicesAdvancedConfiguration Register<TService>()
            where TService : class
        {
            return Register<TService, TService, TService>();
        }

        /// <summary>
        /// Registers a particular service with a particular implementation.
        /// </summary>
		/// <typeparam name="TService">The common service interface shared by the implementations.</typeparam>
		/// <typeparam name="TImpl">The service implementation to use.</typeparam>
        public ServicesAdvancedConfiguration Register<TService, TImpl>()
			where TService : class
			where TImpl : class, TService
		{
			return Register<TService, TImpl, TImpl>();
		}

		/// <summary>
		/// Registers a one or another service implementation depending on whether the IntegrationDb is disabled or not.
		/// </summary>
		/// <typeparam name="TService">The common service interface shared by the implementations.</typeparam>
		/// <typeparam name="TIntegrationDbEnabledImpl">The service implementation to use when we have an IntegrationDb available.</typeparam>
		/// <typeparam name="TIntegrationDbDisabledImpl">The (fallback) service implementation to use when IntegrationDb is disabled.</typeparam>
	    public ServicesAdvancedConfiguration Register<TService, TIntegrationDbEnabledImpl, TIntegrationDbDisabledImpl>()
			where TService : class
			where TIntegrationDbEnabledImpl : class, TService
			where TIntegrationDbDisabledImpl : class, TService
	    {
            // TODO: Test at denne også erstatter en service som er registreret by convention
            // TODO: Giv mulighed for at påvirke Registration / Singleton / ala public InstanceInstaller(T instance, Action<ComponentRegistration<T>> registration = null)

            _registrations[typeof (TService)] = new Registration(
				kernel => Installer.Service<TService, TIntegrationDbEnabledImpl>(),
                kernel => Installer.Service<TService, TIntegrationDbDisabledImpl>());

		    return this;
	    }

		public ServicesAdvancedConfiguration Register<TService>(Func<IKernel, TService> instance)
			where TService : class
		{
			return Register(instance, instance);
		}

        /// <summary>
        /// Registers a one or another service implementation depending on whether the IntegrationDb is disabled or not.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="integrationDbEnabledInstance">The service implementation instance to use when we have an IntegrationDb available.</param>
        /// <param name="integrationDbDisabledInstance">The (fallback) service implementation instance to use when IntegrationDb is disabled.</param>
        public ServicesAdvancedConfiguration Register<TService>(Func<IKernel, TService> integrationDbEnabledInstance, Func<IKernel, TService> integrationDbDisabledInstance)
			where TService : class
		{
		    if (integrationDbEnabledInstance == null) throw new ArgumentNullException(nameof(integrationDbEnabledInstance));
		    if (integrationDbDisabledInstance == null) throw new ArgumentNullException(nameof(integrationDbDisabledInstance));

            _registrations[typeof(TService)] = new Registration(
                kernel => Installer.Instance(integrationDbEnabledInstance(kernel)), 
				kernel => Installer.Instance(integrationDbDisabledInstance(kernel)));

			return this;
	    }

        public ServicesAdvancedConfiguration Install(params IWindsorInstaller[] installers)
        {
            if (installers != null)
                _installers.AddRange(installers.SkipNulls());

            return this;
        }

        private class Registration
        {
            private readonly Func<IKernel, IWindsorInstaller> _enabled;
            private readonly Func<IKernel, IWindsorInstaller> _disabled;

            public Registration(Func<IKernel, IWindsorInstaller> integrationDbEnabled, Func<IKernel, IWindsorInstaller> integrationDbDisabled)
            {
                if (integrationDbEnabled == null) throw new ArgumentNullException(nameof(integrationDbEnabled));
                if (integrationDbDisabled == null) throw new ArgumentNullException(nameof(integrationDbDisabled));

                _enabled = integrationDbEnabled;
                _disabled = integrationDbDisabled;
            }

            public IWindsorInstaller GetInstaller(IKernel kernel, IIntegrationDatabaseConfiguration configuration)
            {
                return configuration.Disabled ? _disabled(kernel) : _enabled(kernel);
            }
        }

        void IInitializable<IWindsorContainer>.Initialized(IWindsorContainer container)
	    {
            // Install these first (as one of them contain the IIntegrationDatabaseConfiguration registration)
            container.Install(_installers.ToArray());

            _installers.Clear();

            var configuration = container.Resolve<IIntegrationDatabaseConfiguration>();

	        foreach (Registration registration in _registrations.Values)
	            container.Install(registration.GetInstaller(container.Kernel, configuration));
        }
    }
}