using System;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Model;

namespace Vertica.Integration.UCommerce.Maintenance
{
    internal static class MaintenanceTaskExtensions
    {
        private const string Name = "UCommerceMaintenanceConfiguration_4B3457979EDD4DE5BEB3087A29DB0A58";

        public static UCommerceMaintenanceConfiguration EnsureConfiguration(this ITaskExecutionContext<MaintenanceWorkItem> context, IConfigurationService configurationService)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (configurationService == null) throw new ArgumentNullException(nameof(configurationService));

            var configuration = context.TypedBag<UCommerceMaintenanceConfiguration>(Name);

            if (configuration != null)
                return configuration;

            return context.TypedBag(Name, configurationService.Get<UCommerceMaintenanceConfiguration>());
        }
    }
}