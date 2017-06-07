using System;

namespace Vertica.Integration.Tests.Infrastructure
{
    public static class ApplicationConfigurationTestingExtensions
    {
        public static ApplicationConfiguration ConfigureForUnitTest(this ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            return application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb.Disable()))
                .Logging(logging => logging
                    .TextWriter());
        }
    }
}