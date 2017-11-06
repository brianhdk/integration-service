using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.IO;

namespace Experiments.Console.Castle.Windsor.CustomInstaller
{
    public static class Demo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .Services(services => services
                    .Advanced(advanced => advanced
                        // We'll override where to read RuntimeSettings from - just for demo purposes
                        // It's recommended to configure these in the app.config instead - which is the default behaviour
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            // Specify our environment.
                            .Set("Environment", ApplicationEnvironment.Production))
                        .Install(new MyCustomInstaller())))))
            {
                var someService = context.Resolve<ISomeService>();

                var writer = context.Resolve<IConsoleWriter>();

                writer.WriteLine(someService.DoStuff());
            }
        }
    }

    public interface ISomeService
    {
        string DoStuff();
    }

    internal class DummySomeService : ISomeService
    {
        public string DoStuff()
        {
            return "Dummy";
        }
    }

    internal class RealSomeService : ISomeService
    {
        public string DoStuff()
        {
            return "The real stuff";
        }
    }

    public class MyCustomInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component
                .For<ISomeService>()
                .UsingFactoryMethod<ISomeService>(kernel =>
                {
                    var settings = kernel.Resolve<IRuntimeSettings>();

                    if (settings.Environment.Equals(ApplicationEnvironment.Production))
                        return kernel.Resolve<RealSomeService>();

                    return kernel.Resolve<DummySomeService>();
                }));

            container.Register(Component.For<RealSomeService>());
            container.Register(Component.For<DummySomeService>());
        }
    }
}