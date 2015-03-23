using System;

namespace Vertica.Integration.Azure
{
    public static class PortalExtensions
    {
        public static ApplicationConfiguration UseAzure(this ApplicationConfiguration builder, Action<AzureConfiguration> azure = null)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            var configuration = new AzureConfiguration();

            if (azure != null)
                azure(configuration);

            builder.AddCustomInstallers(configuration.CustomInstallers);

            return builder;
        }
    }
}