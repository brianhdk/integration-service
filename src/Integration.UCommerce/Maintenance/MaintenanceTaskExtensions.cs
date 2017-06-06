using System;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure.Configuration;

namespace Vertica.Integration.UCommerce.Maintenance
{
    internal static class MaintenanceTaskExtensions
    {
        private const string Name = "UCommerceMaintenanceConfiguration_4B3457979EDD4DE5BEB3087A29DB0A58";

        public static UCommerceMaintenanceConfiguration EnsureConfiguration(this MaintenanceWorkItem workItem, IConfigurationService configurationService)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));
            if (configurationService == null) throw new ArgumentNullException(nameof(configurationService));

            var configuration = workItem[Name] as UCommerceMaintenanceConfiguration;

            if (configuration == null)
            {
                workItem[Name] = 
                    configuration = 
                        configurationService.Get<UCommerceMaintenanceConfiguration>();
            }

            return configuration;
        }
    }
}