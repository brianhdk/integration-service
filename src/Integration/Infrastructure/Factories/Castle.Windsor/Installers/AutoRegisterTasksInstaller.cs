using System;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
    internal class AutoRegisterTasksInstaller : IWindsorInstaller
    {
        private readonly Assembly[] _scanAssemblies;
        private readonly Type[] _skipTypes;

        public AutoRegisterTasksInstaller(Assembly[] scanAssemblies, Type[] skipTypes)
        {
            _scanAssemblies = scanAssemblies ?? new Assembly[0];
            _skipTypes = skipTypes ?? new Type[0];
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            foreach (Assembly assembly in _scanAssemblies)
            {
                container.Register(
                    Classes.FromAssembly(assembly)
                        .BasedOn<Task>()
                        .Unless(_skipTypes.Contains)
                        .Configure(configure => { configure.Named(configure.Implementation.Name); } )
                        .WithServiceDefaultInterfaces());
            }
        }
    }
}