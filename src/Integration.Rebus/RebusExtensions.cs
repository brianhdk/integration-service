using System;
using Vertica.Integration.Domain.LiteServer;

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
				RebusConfiguration configuration = 
                    extensibility.Register(() => new RebusConfiguration(application));

				rebus(configuration);
	        });
        }

        public static LiteServerConfiguration AddRebus(this LiteServerConfiguration liteServer)
        {
            if (liteServer == null) throw new ArgumentNullException(nameof(liteServer));

            UseRebus(liteServer.Application, rebus => rebus.AddToLiteServer());

            return liteServer;
        }
    }
}