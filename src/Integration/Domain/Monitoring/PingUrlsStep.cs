using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Utilities;
using Response = System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage>;

namespace Vertica.Integration.Domain.Monitoring
{
    public class PingUrlsStep : Step<MonitorWorkItem>
    {
        public override Execution ContinueWith(ITaskExecutionContext<MonitorWorkItem> context)
        {
            if (!context.WorkItem.Configuration.PingUrls.ShouldExecute)
                return Execution.StepOver;

            return Execution.Execute;
        }

        public override void Execute(ITaskExecutionContext<MonitorWorkItem> context)
        {
            Uri[] urls = ParseUrls(context.WorkItem.Configuration.PingUrls.Urls, context.Log);

            if (urls.Length == 0)
                return;

            try
            {
                Parallel.ForEach(urls, new ParallelOptions { MaxDegreeOfParallelism = 4 }, url =>
                {
                    Stopwatch watch = Stopwatch.StartNew();

                    try
                    {
                        Response response = HttpGet(url, context.WorkItem.Configuration.PingUrls);
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
                    AddException(pingException, context.WorkItem);
            }
        }

        public override string Description => "Pings a number of configurable urls by issuing a simple GET-request to every configured address expecting a http status code OK/200.";

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

        private static void AddException(PingException exception, MonitorWorkItem workItem)
        {
            string message = $@"{exception.Message}

{exception.InnerException.AggregateMessages()}";

            workItem.Add(Time.UtcNow, "Ping Urls", message);
        }

        private static async Response HttpGet(Uri absoluteUri, MonitorConfiguration.PingUrlsConfiguration configuration)
        {
            using (var handler = new HttpClientHandler())
            {
                if (configuration.UseProxy.HasValue)
                    handler.UseProxy = configuration.UseProxy.Value;

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(configuration.MaximumWaitTimeSeconds);

                    return await client.GetAsync(absoluteUri, HttpCompletionOption.ResponseHeadersRead);
                }
            }
        }

        private class PingException : Exception
        {
            public PingException(Uri uri, Stopwatch watch, Exception inner) : base(
	            $"Ping failed for URL {uri} (running for {Math.Round(watch.Elapsed.TotalSeconds, 3)} seconds).", inner)
            {
            }
        }
    }
}