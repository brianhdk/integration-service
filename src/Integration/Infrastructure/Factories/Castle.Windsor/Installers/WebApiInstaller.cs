using System.Reflection;
using System.Web.Http;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Web;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
    internal class WebApiInstaller : IWindsorInstaller
	{
        private readonly WebApiConfiguration _configuration;

        public WebApiInstaller(WebApiConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .Register(Component.For<ITask>()
                .ImplementedBy<WebApiTask>().Named("WebApiTask"));

            foreach (Assembly assembly in _configuration.Assemblies)
            {
                container.Register(
                    Classes.FromAssembly(assembly)
                        .BasedOn<ApiController>()
                        .WithServiceSelf()
                        .LifestyleTransient());
            }

            container.Register(
                Classes.From(_configuration.Controllers)
                    .BasedOn<ApiController>()
                    .WithServiceSelf()
                    .LifestyleTransient());
		}
	}
}