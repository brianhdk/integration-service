using Castle.DynamicProxy;
using Hangfire.Console;
using Hangfire.Server;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Hangfire.Console
{
    internal class HangfireLoggerInterceptor : IInterceptor
    {
        private readonly HangfirePerformContextFactory _factory;

        public HangfireLoggerInterceptor(HangfirePerformContextFactory factory)
        {
            _factory = factory;
        }

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            if (invocation.Method.ReturnType == typeof(ErrorLog))
            {
                var errorLog = invocation.ReturnValue as ErrorLog;

                if (errorLog != null)
                {
                    PerformContext context = _factory.Current.Get();

                    context.SetTextColor(errorLog.Severity == Severity.Error ? ConsoleTextColor.Red : ConsoleTextColor.Yellow);
                    context.WriteLine(" - ID: {0}", errorLog.Id);
                    context.ResetTextColor();
                }
            }
        }
    }
}