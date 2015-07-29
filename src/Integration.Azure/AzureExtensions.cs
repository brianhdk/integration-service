using System;

namespace Vertica.Integration.Azure
{
    public static class AzureExtensions
    {
        public static ApplicationConfiguration UseAzure(this ApplicationConfiguration application, Action<AzureConfiguration> azure = null)
        {
            if (application == null) throw new ArgumentNullException("application");

            var configuration = new AzureConfiguration(application);

            if (azure != null)
                azure(configuration);

            return application;
        }
    }
}