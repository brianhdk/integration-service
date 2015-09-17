using System;

namespace Vertica.Integration.WebApi
{
    public static class WebApiExtensions
    {
        public static ApplicationConfiguration UseWebApi(this ApplicationConfiguration application, Action<WebApiConfiguration> webApi = null)
        {
            if (application == null) throw new ArgumentNullException("application");

			return application.Extensibility(extensibility =>
			{
				var configuration = extensibility.Register(() => new WebApiConfiguration(application));

				if (webApi != null)
					webApi(configuration);
			});
        }
    }
}