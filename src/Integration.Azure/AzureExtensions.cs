using System;

namespace Vertica.Integration.Azure
{
    public static class AzureExtensions
    {
        public static ApplicationConfiguration UseAzure(this ApplicationConfiguration application, Action<AzureConfiguration> azure = null)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

			return application.Extensibility(extensibility =>
			{
				AzureConfiguration configuration = extensibility.Register(() => new AzureConfiguration(application));

				if (azure != null)
					azure(configuration);
			});
        }
    }
}