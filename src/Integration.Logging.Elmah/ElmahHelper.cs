using System;
using Vertica.Integration.Infrastructure.Configuration;

namespace Vertica.Integration.Logging.Elmah
{
    internal static class ElmahHelper
    {
        public static ElmahConfiguration GetElmahConfiguration(this IConfigurationService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            return service.Get<ElmahConfiguration>();
        }
    }
}