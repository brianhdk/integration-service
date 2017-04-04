using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using NUnit.Framework;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Infrastructure.Remote;
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
            Task httpServer;

            var referenceCounter = new ReferenceCounter();

            using (var context = ApplicationContext.Create(application => application
                .Database(database => database.IntegrationDb(integrationDb => integrationDb.Disable()))
                .Tasks(tasks => tasks.Clear())
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Install(referenceCounter)
                        .Install(Install.Service<ITransientThing, TransientThing>(x => x.LifestyleTransient()))
                        .Install(Install.Service<ISingletonThing, SingletonThing>(x => x.LifestyleSingleton()))
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings())))
                .UseWebApi(webApi => webApi.Add<MyTestController>())))
            {
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

                    for (int i = 0; i < 1000; i++)
                    {
                        int result = httpClient
                            .GetAsync($"mytest?id={i}").Result
                            .Content.ReadAsAsync<int>().Result;

                        Assert.That(result, Is.EqualTo(i));
                    }
                }

                int references = referenceCounter.Value;
                Assert.That(references, Is.EqualTo(0));
            }

            httpServer.Wait(2000);
        }

        private class ReferenceCounter : IWindsorInstaller
        {
            private readonly ReferenceCounterFacility _facility;

            public ReferenceCounter()
            {
                _facility = new ReferenceCounterFacility();
            }

            public void Install(IWindsorContainer container, IConfigurationStore store)
            {
                container.AddFacility(_facility);
            }

            public int Value => _facility.Value;

            class ReferenceCounterFacility : AbstractFacility
            {
                public int Value;

                protected override void Init()
                {
                    Kernel.ComponentCreated += ComponentCreated;
                    Kernel.ComponentDestroyed += ComponentDestroyed;
                }

                void ComponentCreated(Castle.Core.ComponentModel model, object instance)
                {
                    if (instance is MyTestController)
                        Interlocked.Increment(ref Value);
                }

                void ComponentDestroyed(Castle.Core.ComponentModel model, object instance)
                {
                    if (instance is MyTestController)
                        Interlocked.Decrement(ref Value);
                }
            }
        }

        private class MyTestController : ApiController
        {
            private readonly ITransientThing _transientThing;
            private readonly ISingletonThing _singletonThing;

            public MyTestController(ITransientThing transientThing, ISingletonThing singletonThing)
            {
                _transientThing = transientThing;
                _singletonThing = singletonThing;
            }

            public IHttpActionResult Get(int id)
            {
                _transientThing.Execute();
                _singletonThing.Execute();

                return Ok(id);
            }
        }

        private interface ITransientThing
        {
            void Execute();
        }

        private class TransientThing : ITransientThing
        {
            public void Execute()
            {
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
    }
}