using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Remote;
using Vertica.Integration.Model;
using Vertica.Utilities_v4;
using Response=System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage>;

namespace Vertica.Integration.Domain.Monitoring
{
    public class PingUrlsStep : Step<MonitorWorkItem>
    {
        private readonly IConfigurationService _configurationService;
        private readonly IHttpClientFactory _httpClientFactory;

        public PingUrlsStep(IConfigurationService configurationService, IHttpClientFactory httpClientFactory)
        {
            _configurationService = configurationService;
            _httpClientFactory = httpClientFactory;
        }

        public override void Execute(MonitorWorkItem workItem, ILog log)
        {
            PingUrlsConfiguration configuration = _configurationService.Get<PingUrlsConfiguration>();

            Uri[] urls = GetUrls(configuration, log);

            if (urls.Length == 0)
                return;

            try
            {
                Parallel.ForEach(urls, new ParallelOptions { MaxDegreeOfParallelism = 4 }, url =>
                {
                    Stopwatch watch = Stopwatch.StartNew();

                    try
                    {
                        Response response  = HttpGet(url, configuration);
                        response.Wait();
                        response.Result.EnsureSuccessStatusCode();
                    }
                    catch (Exception ex)
                    {
                        throw new PingException(url, watch, ex);
                    }
                });
            }
            catch (AggregateException ex)
            {
                PingException[] exceptions = AssertExceptions(ex);

                foreach (PingException pingException in exceptions)
                    AddException(pingException, workItem);
            }
        }

        public override string Description
        {
            get
            {
                return "Pings a number of configurable urls by issuing a simple GET-request to every configured address expecting a http status code OK/200.";
            }
        }

        private static Uri[] GetUrls(PingUrlsConfiguration configuration, ILog log)
        {
            var urls = new List<Uri>();

            foreach (string url in configuration.Urls ?? new string[0])
            {
                Uri absoluteUri;
                if (Uri.TryCreate(url, UriKind.Absolute, out absoluteUri))
                {
                    if (!urls.Contains(absoluteUri))
                        urls.Add(absoluteUri);
                }
                else
                {
                    log.Warning(Target.Service, @"Skipping ping of ""{0}"". Could not parse value as an absolute URL.", url);
                }
            }

            return urls.ToArray();
        }

        private static PingException[] AssertExceptions(AggregateException ex)
        {
            PingException[] pingExceptions = ex.Flatten().InnerExceptions
                .OfType<PingException>()
                .ToArray();

            if (pingExceptions.Length != ex.InnerExceptions.Count)
                throw ex;

            return pingExceptions;
        }

        private static void AddException(PingException exception, MonitorWorkItem workItem)
        {
            string message = String.Format(@"{0}

{1}", exception.Message, exception.InnerException.AggregateMessages());

            workItem.Add(
                Time.UtcNow,
                "MonitorTask-PingUrlsStep",
                message,
                Target.Service);
        }

        private async Response HttpGet(Uri absoluteUri, PingUrlsConfiguration configuration)
        {
            using (HttpClient client = _httpClientFactory.Create())
            {
                client.Timeout = TimeSpan.FromSeconds(configuration.MaximumWaitTimeSeconds);

                return await client.GetAsync(absoluteUri, HttpCompletionOption.ResponseHeadersRead);
            }
        }

        private class PingException : Exception
        {
            public PingException(Uri uri, Stopwatch watch, Exception inner) : base(String.Format("Ping failed for URL {0} (running for {1} seconds).", uri, Math.Round(watch.Elapsed.TotalSeconds, 3)), inner)
            {
            }
        }
    }
}