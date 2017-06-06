using System.Linq;
using Castle.DynamicProxy;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.ConsoleHost
{
    public class WindowsServiceExecuteInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            var arguments = invocation.Arguments.FirstOrDefault() as HostArguments;

            invocation.Proceed();
        }
    }
}