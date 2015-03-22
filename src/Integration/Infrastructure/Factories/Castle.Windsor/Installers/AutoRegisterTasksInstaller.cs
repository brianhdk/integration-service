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
        private readonly AutoRegistredTasksConfiguration _configuration;

        public AutoRegisterTasksInstaller(AutoRegistredTasksConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            foreach (Assembly assembly in _configuration.ScanAssemblies)
            {
                container.Register(
                    Classes.FromAssembly(assembly)
                        .BasedOn<Task>()
                        .Unless(_configuration.Skipped.Contains)
                        .Configure(configure => { configure.Named(configure.Implementation.Name); } )
                        .WithServiceDefaultInterfaces());
            }
        }
    }
}