using System;
using Vertica.Integration.Domain.LiteServer;

namespace Vertica.Integration.WebApi
{
    public static class WebApiExtensions
    {
        public static ApplicationConfiguration UseWebApi(this ApplicationConfiguration application, Action<WebApiConfiguration> webApi = null)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

			return application.Extensibility(extensibility =>
			{
				var configuration = extensibility.Register(() => new WebApiConfiguration(application));

				webApi?.Invoke(configuration);
			});
        }

        public static LiteServerConfiguration AddWebApi(this LiteServerConfiguration liteServer)
        {
            if (liteServer == null) throw new ArgumentNullException(nameof(liteServer));

            UseWebApi(liteServer.Application, webApi => webApi.AddToLiteServer());

            return liteServer;
        }
    }
}