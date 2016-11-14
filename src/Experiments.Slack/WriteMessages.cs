using System;
using System.Threading;
using Vertica.Integration.Domain.LiteServer;

namespace Experiments.Slack
{
    public class WriteMessages : IBackgroundWorker
    {
        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            //context.Console.WriteLine("Hello {0}", context.InvocationCount);

            return context.Wait(TimeSpan.FromSeconds(5));
        }
    }
}