using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Castle.Core;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using NUnit.Framework;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Infrastructure.Remote;
using Vertica.Integration.Tests.Infrastructure.Testing;
using Vertica.Integration.WebApi;
using Vertica.Integration.WebApi.Infrastructure;

namespace Vertica.Integration.Tests.WebApi
{
    [TestFixture(Category = "Integration,Slow")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    public class WebApiMemoryLeakTester
    {
        [Test]
        public void Ensure_ControllerInstances_Gets_Disposed()
        {
            const int iterations = 10;

            Task httpServer;

            var referenceCounter = new ReferenceCounter();

            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .Tasks(tasks => tasks.Clear())
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Install(referenceCounter)
                        .Install(Install.Service<ISingletonThing, SingletonThing>(x => x.LifestyleSingleton()))
                        .Install(Install.Service<IScopedThing, ScopedThing>(x => x.LifestyleScoped()))
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings())))
                .UseWebApi(webApi => webApi.Add<MyTestController>())))
            {
                IWindsorContainer container = (context as ApplicationContext)?.Container;

                if (container == null)
                    throw new InvalidOperationException();

                var shutdown = context.Resolve<IShutdown>();

                var httpServerFactory = context.Resolve<IHttpServerFactory>();

                bool basedOnSettings;
                string url = httpServerFactory.GetOrGenerateUrl(out basedOnSettings);

                httpServer = Task.Factory.StartNew(() =>
                {
                    using (httpServerFactory.Create(url))
                    {
                        shutdown.Token.WaitHandle.WaitOne();
                    }
                }, shutdown.Token);

                using (HttpClient httpClient = context.Resolve<IHttpClientFactory>().Create())
                {
                    httpClient.BaseAddress = new Uri(url);

                    for (int i = 0; i < iterations; i++)
                    {
                        int result = httpClient
                            .GetAsync($"mytest?id={i}").Result
                            .Content.ReadAsAsync<int>().Result;

                        Assert.That(result, Is.EqualTo(i));
                    }
                }

                // Ensure controllers gets disposed
                int controllerInstances = referenceCounter.Facility.ControllerInstances;
                Assert.That(controllerInstances, Is.EqualTo(0));

                // Ensure scoped instances gets disposed
                int scopedInstances = referenceCounter.Facility.ScopedInstances;
                Assert.That(scopedInstances, Is.EqualTo(0));
            }

            httpServer.Wait(TimeSpan.FromSeconds(2));
        }

        private class ReferenceCounter : IWindsorInstaller
        {
            public ReferenceCounter()
            {
                Facility = new ReferenceCounterFacility();
            }

            public void Install(IWindsorContainer container, IConfigurationStore store)
            {
                container.AddFacility(Facility);
            }

            public ReferenceCounterFacility Facility { get; }

            public class ReferenceCounterFacility : AbstractFacility
            {
                public int ControllerInstances;
                public int ScopedInstances;
                
                protected override void Init()
                {
                    Kernel.ComponentCreated += ComponentCreated;
                    Kernel.ComponentDestroyed += ComponentDestroyed;
                }

                private void ComponentCreated(ComponentModel model, object instance)
                {
                    if (instance is MyTestController)
                        Interlocked.Increment(ref ControllerInstances);

                    if (instance is IScopedThing)
                        Interlocked.Increment(ref ScopedInstances);
                }

                private void ComponentDestroyed(ComponentModel model, object instance)
                {
                    if (instance is MyTestController)
                        Interlocked.Decrement(ref ControllerInstances);

                    if (instance is IScopedThing)
                        Interlocked.Decrement(ref ScopedInstances);
                }
            }
        }

        private class MyTestController : ApiController
        {
            private readonly ISingletonThing _singletonThing;
            private readonly IScopedThing _scopedThing;

            public MyTestController(ISingletonThing singletonThing, IScopedThing scopedThing)
            {
                _singletonThing = singletonThing;
                _scopedThing = scopedThing;
            }

            public IHttpActionResult Get(int id)
            {
                _singletonThing.Execute();
                _scopedThing.Execute();

                return Ok(id);
            }
        }

        private interface ISingletonThing
        {
            void Execute();
        }

        private class SingletonThing : ISingletonThing
        {
            public void Execute()
            {
            }
        }

        private interface IScopedThing
        {
            void Execute();
        }

        private class ScopedThing : IScopedThing
        {
            public void Execute()
            {
            }
        }
    }
}