using System.Linq;
using Castle.DynamicProxy;
using Hangfire.Console;
using Hangfire.Server;

namespace Vertica.Integration.Hangfire.Console
{
    internal class HangfireConsoleWriterInterceptor : IInterceptor
    {
        private readonly HangfirePerformContextFactory _factory;

        public HangfireConsoleWriterInterceptor(HangfirePerformContextFactory factory)
        {
            _factory = factory;
        }

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            PerformContext context = _factory.Current.Get();

            if (context != null)
            {
                var message = (string)invocation.Arguments.FirstOrDefault();

                if (message != null)
                {
                    var args = (object[])invocation.Arguments.ElementAtOrDefault(1);

                    if (args != null)
                        message = string.Format(message, args);

                    if (message.Contains("[WARNING]"))
                    {
                        context.SetTextColor(ConsoleTextColor.Yellow);
                    }
                    else if (message.Contains("[ERROR]"))
                    {
                        context.SetTextColor(ConsoleTextColor.Red);
                    }

                    context.WriteLine(message);
                    context.ResetTextColor();
                }
            }
        }
    }
}