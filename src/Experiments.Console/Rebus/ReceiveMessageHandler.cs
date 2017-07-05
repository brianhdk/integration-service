using System.Threading.Tasks;
using Rebus.Handlers;
using Vertica.Integration.Infrastructure.IO;

namespace Experiments.Console.Rebus
{
    internal class ReceiveAnotherMessageHandler : IHandleMessages<long>
    {
        private readonly IConsoleWriter _writer;

        public ReceiveAnotherMessageHandler(IConsoleWriter writer)
        {
            _writer = writer;
        }

        public Task Handle(long message)
        {
            _writer.WriteLine($"Receiving message: {message}");

            return Task.FromResult(true);
        }
    }

    internal class ReceiveMessageHandler : IHandleMessages<string>
    {
        private readonly IConsoleWriter _writer;

        public ReceiveMessageHandler(IConsoleWriter writer)
        {
            _writer = writer;
        }

        public Task Handle(string message)
        {
            _writer.WriteLine($"Receiving message: {message}");

            return Task.FromResult(true);
        }
    }
}