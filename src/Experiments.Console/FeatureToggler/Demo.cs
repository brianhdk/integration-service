using Vertica.Integration;
using Vertica.Integration.Infrastructure.Features;
using Vertica.Integration.Infrastructure.IO;

namespace Experiments.Console.FeatureToggler
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
                        .Register<MyFeature>()))))
            {
                MyFeature myFeature = context.Resolve<MyFeature>();

                IFeatureToggler toggler = context.Resolve<IFeatureToggler>();
                toggler.Disable<MyFeature>();

                myFeature.DoThing();

                toggler.Enable<MyFeature>();

                myFeature.DoThing();
            }
        }
    }

    public class MyFeature
    {
        private readonly IConsoleWriter _console;
        private readonly IFeatureToggler _feature;

        public MyFeature(IConsoleWriter console, IFeatureToggler feature)
        {
            _console = console;
            _feature = feature;
        }

        public void DoThing()
        {
            if (_feature.IsDisabled<MyFeature>())
                return;

            _console.WriteLine("MyFeature doing it's thing...");
        }
    }
}