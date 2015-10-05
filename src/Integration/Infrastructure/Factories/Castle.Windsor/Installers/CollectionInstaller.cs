using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	public class CollectionInstaller<TService> : IWindsorInstaller
	{
		private readonly List<Assembly> _assemblies;
		private readonly List<Type> _ignoreTypes;

		internal CollectionInstaller()
        {
            _assemblies = new List<Assembly>();
            _ignoreTypes = new List<Type>();
        }

		public CollectionInstaller<TService> AddFromAssemblyOfThis<T>()
		{
			_assemblies.Add(typeof(T).Assembly);

			return this;
		}

		public CollectionInstaller<TService> Ignore<T>()
		{
			_ignoreTypes.Add(typeof(T));

			return this;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			foreach (Assembly assembly in _assemblies.Distinct())
			{
				container.Register(Classes.FromAssembly(assembly)
					.BasedOn<TService>().WithServiceFromInterface(typeof(TService))
					.If(classType => !_ignoreTypes.Any(ignoreType => ignoreType.IsAssignableFrom(classType))));
			}

			container.Register(Component.For<IEnumerable<TService>>().UsingFactoryMethod(kernel => kernel.ResolveAll<TService>()));
			container.Register(Component.For<TService[]>().UsingFactoryMethod(kernel => kernel.ResolveAll<TService>()));
		}
	}
}