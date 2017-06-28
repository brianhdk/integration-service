using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;

namespace Vertica.Integration.Tests.Infrastructure.Testing
{
    public class ConsoleWriterQueue
    {
        private readonly Queue<string> _messages;

        public ConsoleWriterQueue()
        {
            _messages = new Queue<string>();
        }

        public void Enqueue(string message)
        {
            Console.WriteLine(message);
            _messages.Enqueue(message);
        }

        public string Dequeue()
        {
            return _messages.Dequeue();
        }

        public int Count => _messages.Count;

        public class RedirectInterceptor : IInterceptor
        {
            private readonly ConsoleWriterQueue _consoleWriterQueue;

            public RedirectInterceptor(ConsoleWriterQueue consoleWriterQueue)
            {
                _consoleWriterQueue = consoleWriterQueue;
            }

            public void Intercept(IInvocation invocation)
            {
                var message = (string)invocation.Arguments.FirstOrDefault();

                if (message != null)
                {
                    var args = (object[])invocation.Arguments.ElementAtOrDefault(1);

                    if (args != null)
                        message = String.Format(message, args);

                    _consoleWriterQueue.Enqueue(message);
                }
            }
        }
    }
}