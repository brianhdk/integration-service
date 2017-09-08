using System;

namespace Vertica.Integration.WebApi.NSwag
{
	public static class NSwagExtensions
	{
		public static WebApiConfiguration WithNSwag(this WebApiConfiguration webApi, Action<NSwagConfiguration> nswag)
		{
			if (webApi == null) throw new ArgumentNullException(nameof(webApi));
		    if (nswag == null) throw new ArgumentNullException(nameof(nswag));

		    webApi.Application.Extensibility(extensibility =>
			{
				var configuration = extensibility.Register(() => new NSwagConfiguration(webApi));

			    nswag(configuration);
			});

			return webApi;
		}
	}
}