using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Model;
using Vertica.Integration.Tests.Infrastructure.Testing;

namespace Vertica.Integration.Tests.Bootstrapping
{
    [TestFixture]
    public class EnvironmentTester
    {
        [Test]
        public void Tasks_ClearWhenProduction_IsEmpty()
        {
            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .Environment(environment => environment
                    .Customize(ApplicationEnvironment.Production, production => production
                        .Tasks(tasks => tasks.Clear()))
                    .Customize(ApplicationEnvironment.Development, development => development
                        .Tasks(tasks => tasks.MonitorTask()))
                    .Fallback(fallback => fallback
                        .Tasks(tasks => tasks.MaintenanceTask()))
                    .OverrideCurrent(ApplicationEnvironment.Production))))
            {
                ITaskFactory factory = context.Resolve<ITaskFactory>();

                ITask[] tasks = factory.GetAll();

                CollectionAssert.IsEmpty(tasks);
            }
        }

        [Test]
        public void Services_CustomEnvironmentRegisterCustomService_ResolvesCorrectService()
        {
            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .Environment(environment => environment
                    .Customize(ApplicationEnvironment.Custom("Custom"), custom => custom
                        .Services(services => services
                            .Advanced(advanced => advanced
                                .Register<ISomeService, CustomService>())))
                    .Fallback(fallback => fallback
                        .Services(services => services
                            .Advanced(advanced => advanced
                                .Register<ISomeService, FallbackService>())))
                    .OverrideCurrent("Custom"))))
            {
                var service = context.Resolve<ISomeService>();

                Assert.That(service, Is.TypeOf<CustomService>());
            }
        }

        [Test]
        public void Services_FallbackEnvironmentRegisterFallbackService_ResolvesCorrectService()
        {
            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .Environment(environment => environment
                    .Customize(ApplicationEnvironment.Custom("Custom"), custom => custom
                        .Services(services => services
                            .Advanced(advanced => advanced
                                .Register<ISomeService, CustomService>())))
                    .Fallback(fallback => fallback
                        .Services(services => services
                            .Advanced(advanced => advanced
                                .Register<ISomeService, FallbackService>())))
                    .OverrideCurrent("Development"))))
            {
                var service = context.Resolve<ISomeService>();

                Assert.That(service, Is.TypeOf<FallbackService>());
            }
        }

        public interface ISomeService
        {
        }

        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
        public class CustomService : ISomeService
        {
        }

        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
        public class FallbackService : ISomeService
        {
        }
    }
}