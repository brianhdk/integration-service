using System;
using Vertica.Integration;

namespace Experiments.Files
{
    public static class ServerExtensions
    {
        public static ApplicationConfiguration UseServer(this ApplicationConfiguration application, Action<ServerConfiguration> server)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));
	        if (server == null) throw new ArgumentNullException(nameof(server));

	        return application.Extensibility(extensibility =>
			{
				var configuration = extensibility.Register(() => new ServerConfiguration(application));

				server(configuration);
			});
        }
    }
}