using System.Threading.Tasks;
using Rebus.Handlers;
using Vertica.Integration.Infrastructure.IO;

namespace Experiments.Console.Rebus
{
    public class ReceiveMessageHandler : IHandleMessages<string>
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