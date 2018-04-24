using System;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure.Remote;
using Vertica.Integration.WebApi;

namespace Experiments.Console.WebApi.Controllers
{
    public static class Demo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .Logging(logging => logging
                    .TextWriter(logger => logger.Detailed()))
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            // Specify which URL WebAPI should listen on.
                            .Set("WebApi.Url", "http://localhost:8154"))))
                .UseLiteServer(liteServer => liteServer
                    .AddWebApi()
                    .AddWorker<RequestWebApiWorker>())
                .UseWebApi(webApi => webApi
                    .Add<HelloController>()
                    .Add<AnotherController>())))
            {
                // Fires up LiteServer
                context.Execute(nameof(LiteServerHost));
            }
        }

        public class HelloController : ApiController
        {
            public IHttpActionResult Get()
            {
                return Ok("Hello");
            }
        }

        [RoutePrefix("custom/url")]
        public class AnotherController : ApiController
        {
            [HttpGet]
            [Route("route")]
            public IHttpActionResult MyMethodOnACompletelyCustomRoute()
            {
                return Ok("You found me");
            }

            [HttpGet]
            [Route("route/different")]
            public IHttpActionResult OnADifferentRoute()
            {
                return Ok("You found me too");
            }

            [HttpGet]
            [Route("route/exception")]
            public IHttpActionResult ThrowException()
            {
                throw new InvalidOperationException("Some exception");
            }
        }

        public class RequestWebApiWorker : IBackgroundWorker
        {
            private readonly HttpClient _httpClient;
            private readonly string _webApiUrl;

            public RequestWebApiWorker(IHttpClientFactory httpClientFactory, IRuntimeSettings settings)
            {
                _httpClient = httpClientFactory.Create();
                _webApiUrl = settings["WebApi.Url"];
            }

            public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
            {
                Request(context, token, "hello");
                Request(context, token, "custom/url/route");
                Request(context, token, "custom/url/route/different");

                return context.Wait(TimeSpan.FromSeconds(5));
            }

            private void Request(BackgroundWorkerContext context, CancellationToken token, string path)
            {
                string url = $"{_webApiUrl}/{path}";

                context.Console.WriteLine("[HttpGet]: {0}", url);

                using (HttpResponseMessage hello = _httpClient.GetAsync(url, token).Result)
                {
                    context.Console.WriteLine($" => {hello.Content.ReadAsStringAsync().Result}");
                }
            }
        }
    }
}