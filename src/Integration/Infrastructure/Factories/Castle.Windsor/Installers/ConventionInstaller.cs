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
        private readonly Assembly[] _assemblies;
        private readonly Type[] _ignoreTypes;

        public ConventionInstaller(IEnumerable<Assembly> assemblies, params Type[] ignoreTypes)
        {
            _assemblies = (assemblies ?? Enumerable.Empty<Assembly>()).Distinct().ToArray();

            _ignoreTypes = (ignoreTypes ?? Enumerable.Empty<Type>()).Concat(new[]
            {
                typeof(IWindsorInstaller)

            }).Distinct().ToArray();
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            foreach (var assembly in _assemblies)
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