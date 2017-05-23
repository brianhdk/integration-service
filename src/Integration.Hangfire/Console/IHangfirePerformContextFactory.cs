using Castle.MicroKernel;

namespace Vertica.Integration.Hangfire.Console
{
    internal class HangfirePerformContextFactory
    {
        private readonly IKernel _kernel;

        public HangfirePerformContextFactory(IKernel kernel)
        {
            _kernel = kernel;
        }

        public HangfirePerThreadPerformContext Current => _kernel.Resolve<HangfirePerThreadPerformContext>();
    }
}