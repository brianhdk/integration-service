using System;
using System.Threading;
using Rebus.Bus;
using Vertica.Integration.Domain.LiteServer;

namespace Experiments.Console.Rebus
{
    internal class SendMessageWorker : IBackgroundWorker, IRestartable
    {
        private readonly IBus _bus;

        public SendMessageWorker(IBus bus)
        {
            _bus = bus;
        }

        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            context.Console.WriteLine($"Sending message from Worker {context.InvocationCount}");

            _bus.Send($"Hello {context.InvocationCount}").Wait(token);

            return context.Wait(TimeSpan.FromSeconds(5));
        }

        public bool ShouldRestart(RestartableContext context)
        {
            return false;
        }

        public override string ToString()
        {
            return nameof(SendMessageWorker);
        }
    }
}