using System;
using Vertica.Integration.Model.Web;

namespace Vertica.Integration.Portal
{
    public static class PortalExtensions
    {
        public static ApplicationConfiguration UsePortal(this ApplicationConfiguration builder, Action<PortalConfiguration> portal = null)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            var configuration = new PortalConfiguration();

            if (portal != null)
                portal(configuration);

            builder.WebApi(x => x
                .Remove<HomeController>()
                .Scan(typeof (PortalExtensions).Assembly));

            return builder;
        }
    }
}