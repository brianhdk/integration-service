using Hangfire.Dashboard;

namespace Experiments.Console.Hangfire
{
    public class AllowAllAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}