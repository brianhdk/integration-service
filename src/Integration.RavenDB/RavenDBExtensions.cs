using System;

namespace Vertica.Integration.RavenDB
{
    public static class RavenDbExtensions
    {
        public static ApplicationConfiguration UseRavenDb(this ApplicationConfiguration application, Action<RavenDbConfiguration> ravenDb)
        {
            if (application == null) throw new ArgumentNullException("application");
            if (ravenDb == null) throw new ArgumentNullException("ravenDb");

			return application.Extensibility(extensibility =>
			{
				RavenDbConfiguration configuration = extensibility.Register(() => new RavenDbConfiguration(application));

				ravenDb(configuration);
			});
        }
    }
}