using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	internal class TypeInstaller<TService> : IWindsorInstaller where TService : class
	{
		private readonly Type _type;
		private readonly Action<ComponentRegistration<TService>> _registration;

		public TypeInstaller(Type type, Action<ComponentRegistration<TService>> registration = null)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			_type = type;
			_registration = registration;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			ComponentRegistration<TService> registration = Component.For<TService>().ImplementedBy(_type);

			_registration?.Invoke(registration);

			container.Register(registration);
		}
	}
}