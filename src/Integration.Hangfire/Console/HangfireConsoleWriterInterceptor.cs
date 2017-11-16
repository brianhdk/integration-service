using System.Linq;
using Castle.DynamicProxy;
using Hangfire.Console;
using Hangfire.Server;

namespace Vertica.Integration.Hangfire.Console
{
    internal class HangfireConsoleWriterInterceptor : IInterceptor
    {
        private readonly IHangfirePerformContextProvider _provider;

        public HangfireConsoleWriterInterceptor(IHangfirePerformContextProvider provider)
        {
            _provider = provider;
        }

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            PerformContext context = _provider.Current;

            if (context != null)
            {
                var message = (string)invocation.Arguments.FirstOrDefault();

                if (message != null)
                {
                    var args = (object[])invocation.Arguments.ElementAtOrDefault(1);

                    if (args != null && args.Length > 0)
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