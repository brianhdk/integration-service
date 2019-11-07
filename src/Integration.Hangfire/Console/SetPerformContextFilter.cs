using Castle.MicroKernel;
using Hangfire.Server;

namespace Vertica.Integration.Hangfire.Console
{
    internal class SetPerformContextFilter : IServerFilter
    {
        private readonly IKernel _kernel;

        public SetPerformContextFilter(IKernel kernel)
        {
            _kernel = kernel;
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            if (filterContext == null)
                return;

            var context = _kernel.Resolve<HangfirePerThreadPerformContext>();

            context.Value = filterContext;
        }

        public void OnPerformed(PerformedContext filterContext)
        {
        }
    }
}