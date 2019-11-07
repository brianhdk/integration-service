using System;
using Castle.MicroKernel.Registration;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	public static class Install
	{
		public static ConventionInstaller ByConvention => new ConventionInstaller();

		public static CollectionInstaller<TService> Collection<TService>()
		{
			return new CollectionInstaller<TService>();
		}

		public static IWindsorInstaller Instance<T>(T instance, Action<ComponentRegistration<T>> registration = null) where T : class
		{
			return new InstanceInstaller<T>(instance, registration);
		}

	    public static IWindsorInstaller Type(Type serviceType, Type implementationType, Action<ComponentRegistration<object>> registration = null)
	    {
	        return new TypeInstaller(serviceType, implementationType, registration);
	    }

        public static IWindsorInstaller Type<TService>(Type type, Action<ComponentRegistration<TService>> registration = null) where TService : class
		{
			return new TypeInstaller<TService>(type, registration);
		}

		public static IWindsorInstaller Service<TService>(Action<ComponentRegistration<TService>> registration = null) 
            where TService : class
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