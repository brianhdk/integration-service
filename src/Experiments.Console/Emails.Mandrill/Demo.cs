using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel;
using Vertica.Integration;
using Vertica.Integration.Emails.Mandrill;
using Vertica.Integration.Emails.Mandrill.Infrastructure;
using Vertica.Integration.Infrastructure.IO;

namespace Experiments.Console.Emails.Mandrill
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
                            // Specify your API key
                            .Set("Mandrill.ApiKey", @"TODO"))))
                .UseMandrill(mandrill => mandrill
                    .WithHttpMessageHandler(kernel => new LoggingHandler(kernel, new HttpClientHandler()))
                    .ConfiguredBy(by => by.RuntimeSettings())
                )
            ))
            {
                var apiService = context.Resolve<IMandrillApiService>();

                Reject[] rejects = apiService
                    .RequestAsync<GetRejects, Reject[]>("rejects/list", new GetRejects())
                    .Result;

                context.Resolve<IConsoleWriter>().WriteLine(
                    string.Join(Environment.NewLine, rejects
                        .Select(x => $"{x.Email} - {x.reason} - {x.last_event_at}")));

                Sender[] senders = apiService
                    .RequestAsync<GetSendersList, Sender[]>("senders/list", new GetSendersList())
                    .Result;

                context.Resolve<IConsoleWriter>().WriteLine(
                    string.Join(Environment.NewLine, senders
                        .Select(x => $"{x.Address}")));
            }
        }

        private class GetSendersList : MandrillApiRequest
        {
        }

        private class GetRejects : MandrillApiRequest
        {
        }

        private class Reject
        {
            public string Email { get; set; }
            public string reason { get; set; }
            public string detail { get; set; }
            public DateTime last_event_at { get; set; }
        }

        private class Sender
        {
            public string Address { get; set; }
        }
    }

    public class LoggingHandler : DelegatingHandler
    {
        private readonly IConsoleWriter _console;

        public LoggingHandler(IKernel kernel, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            _console = kernel.Resolve<IConsoleWriter>();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _console.WriteLine("Request:");

            _console.WriteLine(request.ToString());

            if (request.Content != null)
                _console.WriteLine(await request.Content.ReadAsStringAsync());
            
            _console.WriteLine();

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            _console.WriteLine("Response:");
            _console.WriteLine(response.ToString());

            if (response.Content != null)
                _console.WriteLine(await response.Content.ReadAsStringAsync());
            
            _console.WriteLine();

            return response;
        }
    }

    public class MyImplementation : IMandrillSettingsProvider
    {
        public MandrillSettings Get()
        {
            return new MandrillSettings
            {
                ApiKey = "TODO"
            };
        }
    }
}