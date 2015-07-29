using System;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
    internal class HostsInstaller : IWindsorInstaller
    {
        private readonly Type[] _addHosts;
        private readonly Type[] _skipHosts;

        public HostsInstaller(Type[] addHosts, Type[] skipHosts)
        {
            _addHosts = addHosts ?? new Type[0];
            _skipHosts = skipHosts ?? new Type[0];
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            foreach (Type add in _addHosts.Except(_skipHosts).Distinct())
            {
                container.Register(
                    Component.For<IHost>()
                        .ImplementedBy(add));
            }
        }
    }
}