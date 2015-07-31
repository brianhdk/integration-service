using System;

namespace Vertica.Integration.RavenDB
{
    public static class RavenDBExtensions
    {
        public static ApplicationConfiguration UseRavenDB(this ApplicationConfiguration application, Action<RavenDBConfiguration> ravenDB)
        {
            if (application == null) throw new ArgumentNullException("application");
            if (ravenDB == null) throw new ArgumentNullException("ravenDB");

			return application.Extensibility(extensibility =>
			{
				RavenDBConfiguration configuration = extensibility.Cache(() => new RavenDBConfiguration(application));

				ravenDB(configuration);
			});
        }
    }
}