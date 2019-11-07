using Hangfire.Dashboard;

namespace Experiments.Website
{
    public class HangfireAllowAllAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}