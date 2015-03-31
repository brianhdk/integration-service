using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
    public class ConventionInstaller : IWindsorInstaller
    {
        private readonly List<Assembly> _assemblies;
        private readonly List<Type> _ignoreTypes;

        public ConventionInstaller()
        {
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

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            foreach (Assembly assembly in _assemblies.Distinct())
            {
                container.Register(
                    Classes.FromAssembly(assembly)
                        .Pick()
                        .If(classType =>
                            classType.GetInterfaces().Any(classInterface => _assemblies.Contains(classInterface.Assembly)) &&
                            !_ignoreTypes.Any(ignoreType => ignoreType.IsAssignableFrom(classType)))
                        .WithService.DefaultInterfaces()
                        .Configure(registration => ConfigureRegistration(registration)));
            }
        }

        private static ComponentRegistration<object> ConfigureRegistration(ComponentRegistration registration)
        {
            if (!Attribute.IsDefined(registration.Implementation, typeof(LifestyleAttribute)))
                return registration.LifeStyle.Singleton;

            return registration;
        }
    }
}