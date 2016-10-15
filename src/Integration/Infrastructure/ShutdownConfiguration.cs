using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Infrastructure.IO;

namespace Vertica.Integration.Infrastructure
{
	public class ShutdownConfiguration : IInitializable<IWindsorContainer>
	{
        private readonly Action<ComponentRegistration<IWaitForShutdownRequest>> _registration = component =>
        {
            component.LifestyleSingleton();
        };

	    private IWindsorInstaller _installer;

	    internal ShutdownConfiguration(AdvancedConfiguration advanced)
		{
	        if (advanced == null) throw new ArgumentNullException(nameof(advanced));

	        Advanced = advanced;
		}

		public AdvancedConfiguration Advanced { get; }

		/// <summary>
		/// Registers a custom implementation to take care of waiting for shutdown requests.
		/// </summary>
		public ShutdownConfiguration Custom<T>(Action<ComponentRegistration<IWaitForShutdownRequest>> registration = null)
			where T : class, IWaitForShutdownRequest
		{
		    _installer = Install.Type(typeof(T), registration ?? _registration);

			return this;
		}

		/// <summary>
		/// Registers a custom implementation to take care of waiting for shutdown requests.
		/// </summary>
		public ShutdownConfiguration Custom(IWaitForShutdownRequest instance)
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));

		    _installer = Install.Instance(instance, _registration);

            return this;
		}

		void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
		{
		    if (_installer == null)
		    {
		        if (AzureWebJobShutdownRequest.IsRunningOnAzure())
		        {
		            Custom<AzureWebJobShutdownRequest>();
		        }
		        else
		        {
                    Custom<WaitForEscapeKey>();
		        }
		    }

		    container.Install(_installer);
		}
	}
}