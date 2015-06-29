using System;

namespace Vertica.Integration.Experiments
{
    public static class ConfigurationExtensions
    {
        public static ApplicationConfiguration Fast(this ApplicationConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            return configuration
                .Database(database => database.DisableIntegrationDb())
                .Logging(logging => logging.NullLogger());
        }

        public static ApplicationConfiguration Void(this ApplicationConfiguration configuration)
        {
            return configuration;
        }
    }
}