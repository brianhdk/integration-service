using Castle.MicroKernel;
using Hangfire.Server;

namespace Vertica.Integration.Hangfire.Console
{
    internal class HangfirePerformContextProvider : IHangfirePerformContextProvider
    {
        private readonly IKernel _kernel;

        public HangfirePerformContextProvider(IKernel kernel)
        {
            _kernel = kernel;
        }

        public PerformContext Current
        {
            get
            {
                var context = _kernel.Resolve<HangfirePerThreadPerformContext>();

                return context.Value;
            }
        }
    }
}