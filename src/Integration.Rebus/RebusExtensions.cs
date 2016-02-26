using System;

namespace Vertica.Integration.Rebus
{
    public static class RebusExtensions
    {
        public static ApplicationConfiguration UseRebus(this ApplicationConfiguration application, Action<RebusConfiguration> rebus)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));
	        if (rebus == null) throw new ArgumentNullException(nameof(rebus));

	        return application.Extensibility(extensibility =>
	        {
				RebusConfiguration configuration = extensibility.Register(() =>
		        {
			        application.Hosts(x => x.Host<RebusHost>());

			        return new RebusConfiguration(application);
		        });

				rebus(configuration);
	        });
        }
    }
}