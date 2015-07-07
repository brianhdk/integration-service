using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Remote;
using Vertica.Integration.Model;
using Vertica.Utilities_v4;
using Response= System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage>;

namespace Vertica.Integration.Domain.Monitoring
{
    public class PingUrlsStep : Step<MonitorWorkItem>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PingUrlsStep(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public override Execution ContinueWith(MonitorWorkItem workItem)
        {
            if (!workItem.Configuration.PingUrls.ShouldExecute)
                return Execution.StepOver;

            return Execution.Execute;
        }

        public override void Execute(MonitorWorkItem workItem, ITaskExecutionContext context)
        {
            Uri[] urls = ParseUrls(workItem.Configuration.PingUrls.Urls, context.Log);

            if (urls.Length == 0)
                return;

            try
            {
                Parallel.ForEach(urls, new ParallelOptions { MaxDegreeOfParallelism = 4 }, url =>
                {
                    Stopwatch watch = Stopwatch.StartNew();

                    try
                    {
                        Response response = HttpGet(url, workItem.Configuration.PingUrls.MaximumWaitTimeSeconds);
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

        private static Uri[] ParseUrls(string[] urls, ILog log)
        {
            var result = new List<Uri>();

            foreach (string url in urls)
            {
                Uri absoluteUri;
                if (Uri.TryCreate(url, UriKind.Absolute, out absoluteUri))
                {
                    if (!result.Contains(absoluteUri))
                        result.Add(absoluteUri);
                }
                else
                {
                    log.Warning(Target.Service, @"Skipping ping of ""{0}"". Could not parse value as an absolute URL.", url);
                }
            }

            return result.ToArray();
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

        private void AddException(PingException exception, MonitorWorkItem workItem)
        {
            string message = String.Format(@"{0}

{1}", exception.Message, exception.InnerException.AggregateMessages());

            workItem.Add(Time.UtcNow, this.Name(), message);
        }

        private async Response HttpGet(Uri absoluteUri, uint maximumWaitTimeSeconds)
        {
            using (HttpClient client = _httpClientFactory.Create())
            {
                client.Timeout = TimeSpan.FromSeconds(maximumWaitTimeSeconds);

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