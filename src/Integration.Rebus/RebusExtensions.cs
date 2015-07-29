using System;

namespace Vertica.Integration.Rebus
{
    public static class RebusExtensions
    {
        public static ApplicationConfiguration UseRebus(this ApplicationConfiguration builder, Action<RebusConfiguration> rebus)
        {
            if (builder == null) throw new ArgumentNullException("builder");
	        if (rebus == null) throw new ArgumentNullException("rebus");

	        var configuration = new RebusConfiguration(builder);

	        rebus(configuration);

	        builder.Hosts(x => x.Host<RebusHost>());

            return builder;
        }
    }
}