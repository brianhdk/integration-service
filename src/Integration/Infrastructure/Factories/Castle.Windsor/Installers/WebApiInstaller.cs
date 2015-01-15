using System.Reflection;
using System.Web.Http;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Web;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
    public class WebApiInstaller : IWindsorInstaller
	{
        private readonly Assembly[] _assemblies;

        public WebApiInstaller(params Assembly[] assemblies)
        {
            _assemblies = assemblies ?? new Assembly[0];
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .Register(Component.For<ITask>()
                .ImplementedBy<WebApiTask>().Named("WebApiTask"));

            foreach (var assembly in _assemblies)
            {
                container.Register(
                    Classes.FromAssembly(assembly)
                        .BasedOn<ApiController>()
                        .WithServiceSelf()
                        .LifestyleTransient());
            }
		}
	}
}