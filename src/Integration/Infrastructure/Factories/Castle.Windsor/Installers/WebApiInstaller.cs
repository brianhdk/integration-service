using System.Linq;
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

            foreach (Assembly assembly in _configuration.ScanAssemblies)
            {
                container.Register(
                    Classes.FromAssembly(assembly)
                        .BasedOn<ApiController>()
                        .If(x => !_configuration.RemoveControllers.Contains(x))
                        .WithServiceSelf()
                        .WithServiceBase()
                        .LifestyleTransient());
            }

            container.Register(
                Classes.From(_configuration.AddControllers)
                    .BasedOn<ApiController>()
                    .WithServiceSelf()
                    .WithServiceBase()
                    .LifestyleTransient());
		}
	}
}