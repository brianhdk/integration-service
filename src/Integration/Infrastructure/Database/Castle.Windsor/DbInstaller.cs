using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.NHibernate;

namespace Vertica.Integration.Infrastructure.Database.Castle.Windsor
{
    internal class DbInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component
                    .For<IDbFactory>()
                    .AsFactory());

            container.Register(
                Component
                    .For<IDb>()
                    .UsingFactoryMethod(kernel => new Db(kernel.Resolve<ISessionFactoryProvider>().SessionFactory.OpenStatelessSession()))
                    .LifestyleTransient());
        }
    }
}