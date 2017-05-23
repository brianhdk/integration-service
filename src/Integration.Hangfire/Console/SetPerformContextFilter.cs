using Hangfire.Server;

namespace Vertica.Integration.Hangfire.Console
{
    internal class SetPerformContextFilter : IServerFilter
    {
        private readonly HangfirePerformContextFactory _factory;

        public SetPerformContextFilter(HangfirePerformContextFactory factory)
        {
            _factory = factory;
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            _factory.Current.Set(filterContext);
        }

        public void OnPerformed(PerformedContext filterContext)
        {
        }
    }
}