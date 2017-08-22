using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	public sealed class ConventionInstaller : IWindsorInstaller
    {
	    private readonly Action<ComponentRegistration> _registration;
	    private readonly List<Assembly> _assemblies;
        private readonly List<Type> _ignoreTypes;

		internal ConventionInstaller(Action<ComponentRegistration> registration = null)
		{
		    _registration = registration ?? (x => x.LifestyleSingleton().IsFallback());
		    _assemblies = new List<Assembly>();
            _ignoreTypes = new List<Type>();
        }

        public ConventionInstaller AddFromAssemblyOfThis<T>()
        {
            _assemblies.Add(typeof (T).Assembly);

            return this;
        }

        public ConventionInstaller Ignore<T>()
        {
            _ignoreTypes.Add(typeof (T));

            return this;
        }

        void IWindsorInstaller.Install(IWindsorContainer container, IConfigurationStore store)
        {
            Func<Type, Type, bool> isConvention = (@class, @interface) =>
                @interface.Assembly.Equals(@class.Assembly) &&
                (@interface.Namespace ?? string.Empty).Equals(@class.Namespace) &&
                @interface.Name.Equals($"I{@class.Name}");

            foreach (Assembly assembly in _assemblies.Distinct())
            {
                container.Register(
                    Classes.FromAssembly(assembly)
                        .Pick()
                        .If(@class =>
                            @class.GetInterfaces().Any(@interface => isConvention(@class, @interface)) &&
                            !_ignoreTypes.Any(ignoreType => @class == ignoreType || ignoreType.IsAssignableFrom(@class)))
                        .WithService.Select((@class, baseTypes) => new[] { @class.GetInterfaces().First(@interface => isConvention(@class, @interface)) })
                        .Configure(_registration));
            }
        }
    }
}