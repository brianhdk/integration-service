using System;
using System.IO;
using System.Threading;
using Rebus.Bus;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Domain.LiteServer.Servers.IO;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Extensions;

namespace Experiments.Console.Rebus
{
    internal class SendMessageServer : FileWatcherServer
    {
        private readonly IBus _bus;
        private readonly CancellationToken _token;

        public SendMessageServer(IBus bus, IShutdown shutdown)
        {
            _bus = bus;
            _token = shutdown.Token;
        }

        protected override DirectoryInfo PathToMonitor()
        {
            return new DirectoryInfo(@"c:\tmp\tomonitor");
        }

        protected override void ProcessFile(FileInfo file, FileSystemEventArgs args)
        {
            if (file.IsLocked())
                _token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1));

            try
            {
                _bus.Send(file.Length).Wait(_token);
            }
            finally
            {
                file.Delete();
            }
        }

        protected override string Filter => "*.txt";

        protected override void ProcessDirectory(DirectoryInfo directory, FileSystemEventArgs args)
        {
            throw new NotSupportedException();
        }
    }

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