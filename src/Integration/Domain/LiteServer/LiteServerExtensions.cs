using System;

namespace Vertica.Integration.Domain.LiteServer
{
    public static class LiteServerExtensions
    {
        public static ApplicationConfiguration UseLiteServer(this ApplicationConfiguration application, Action<LiteServerConfiguration> server)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));
	        if (server == null) throw new ArgumentNullException(nameof(server));

	        return application.Extensibility(extensibility =>
			{
				var configuration = extensibility.Register(() => new LiteServerConfiguration(application));

				server(configuration);
			});
        }
    }
}