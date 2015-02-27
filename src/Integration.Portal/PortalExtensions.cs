using System;

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

            builder.WebApi(x => x.ScanAssembly(typeof (PortalExtensions).Assembly));
            
            return builder;
        }
    }
}