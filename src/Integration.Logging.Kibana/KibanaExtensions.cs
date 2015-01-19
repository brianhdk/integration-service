using System;
using Vertica.Integration.Logging.Kibana.Infrastructure.Castle.Windsor;

namespace Vertica.Integration.Logging.Kibana
{
    public static class KibanaExtensions
    {
        public static ApplicationConfiguration UseKibana(this ApplicationConfiguration builder, Action<KibanaConfiguration> kibana = null)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            var configuration = new KibanaConfiguration();

            if (kibana != null)
                kibana(configuration);

            builder.AddCustomInstaller(new AzureBlobStorageInstaller(configuration.AzureBlobStorageConnectionStringName));

            return builder;
        }
    }
}