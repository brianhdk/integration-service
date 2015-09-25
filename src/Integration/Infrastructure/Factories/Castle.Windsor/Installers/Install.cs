using System;
using Castle.MicroKernel.Registration;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	public static class Install
	{
		public static ConventionInstaller ByConvention
		{
			get { return new ConventionInstaller(); }
		}

		public static IWindsorInstaller Instance<T>(T instance) where T : class
		{
			return new InstanceInstaller<T>(instance);
		}

		public static IWindsorInstaller Service<TService>(Action<ComponentRegistration<TService>> registration = null) where TService : class
		{
			return new ServiceInstaller<TService>(registration);
		}

		public static IWindsorInstaller Service<TService, TImplementation>(Action<ComponentRegistration<TService>> registration = null) 
			where TService : class 
			where TImplementation : class, TService
		{
			return new ServiceInstaller<TService, TImplementation>(registration);
		}
	}
}