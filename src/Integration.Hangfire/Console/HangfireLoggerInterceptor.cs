using Castle.DynamicProxy;
using Hangfire.Console;
using Hangfire.Server;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Hangfire.Console
{
    internal class HangfireLoggerInterceptor : IInterceptor
    {
        private readonly IHangfirePerformContextProvider _provider;

        public HangfireLoggerInterceptor(IHangfirePerformContextProvider provider)
        {
            _provider = provider;
        }

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            PerformContext context = _provider.Current;

            if (context != null)
            {
                if (invocation.Method.ReturnType == typeof(ErrorLog))
                {
                    if (invocation.ReturnValue is ErrorLog errorLog)
                    {
                        context.SetTextColor(errorLog.Severity == Severity.Error ? ConsoleTextColor.Red : ConsoleTextColor.Yellow);
                        context.WriteLine(" - ID: {0}", errorLog.Id);
                        context.ResetTextColor();
                    }
                }
            }
        }
    }
}