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
    }
}