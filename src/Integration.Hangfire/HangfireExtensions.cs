using System;
using Vertica.Integration.Domain.LiteServer;

namespace Vertica.Integration.Hangfire
{
    public static class HangfireExtensions
    {
        public static ApplicationConfiguration UseHangfire(this ApplicationConfiguration application, Action<HangfireConfiguration> hangfire)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));
	        if (hangfire == null) throw new ArgumentNullException(nameof(hangfire));

	        return application.Extensibility(extensibility =>
			{
				var configuration = extensibility.Register(() => new HangfireConfiguration(application));

				hangfire(configuration);
			});
        }

        public static LiteServerConfiguration AddHangfire(this LiteServerConfiguration liteServer)
        {
            if (liteServer == null) throw new ArgumentNullException(nameof(liteServer));

            UseHangfire(liteServer.Application, hangfire => hangfire.AddToLiteServer());

            return liteServer;
        }
    }
}