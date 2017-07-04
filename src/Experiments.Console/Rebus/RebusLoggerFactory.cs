using System;
using Rebus.Logging;
using Vertica.Integration.Infrastructure.IO;

namespace Experiments.Console.Rebus
{
    public class RebusLoggerFactory : IRebusLoggerFactory
    {
        private readonly IConsoleWriter _console;

        public RebusLoggerFactory(IConsoleWriter console)
        {
            _console = console;
        }

        public ILog GetLogger<T>()
        {
            return new RebusLogger(_console);
        }

        private class RebusLogger : ILog
        {
            private readonly IConsoleWriter _console;

            public RebusLogger(IConsoleWriter console)
            {
                _console = console;
            }

            public void Debug(string message, params object[] objs)
            {
                _console.WriteLine(message, objs);
            }

            public void Info(string message, params object[] objs)
            {
                _console.WriteLine(message, objs);
            }

            public void Warn(string message, params object[] objs)
            {
                _console.WriteLine(message, objs);
            }

            public void Error(Exception exception, string message, params object[] objs)
            {
                _console.WriteLine(message, objs);
            }

            public void Error(string message, params object[] objs)
            {
                _console.WriteLine(message, objs);
            }
        }
    }
}