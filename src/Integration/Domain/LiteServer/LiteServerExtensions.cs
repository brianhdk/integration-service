using System;

namespace Vertica.Integration.Domain.LiteServer
{
    public static class LiteServerExtensions
    {
        public static ApplicationConfiguration UseLiteServer(this ApplicationConfiguration application, Action<LiteServerConfiguration> server = null)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

	        return application.Extensibility(extensibility =>
			{
				var configuration = extensibility.Register(() => new LiteServerConfiguration(application));

				server?.Invoke(configuration);
			});
        }
    }
}