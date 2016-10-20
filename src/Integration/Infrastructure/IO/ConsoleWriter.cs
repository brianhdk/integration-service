using System.IO;

namespace Vertica.Integration.Infrastructure.IO
{
    public class ConsoleWriter : IConsoleWriter
    {
        private readonly TextWriter _writer;

        public ConsoleWriter(TextWriter writer)
        {
            _writer = writer;
        }

        public void WriteLine(string format, params object[] args)
        {
            _writer.WriteLine(format, args);
        }
    }
}