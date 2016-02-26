using System;

namespace Vertica.Integration.WebApi.SignalR
{
	public static class SignalRExtensions
	{
		public static WebApiConfiguration WithSignalR(this WebApiConfiguration webApi, Action<SignalRConfiguration> signalR)
		{
			if (webApi == null) throw new ArgumentNullException(nameof(webApi));
			if (signalR == null) throw new ArgumentNullException(nameof(signalR));

			webApi.Application.Extensibility(extensibility =>
			{
				var configuration = extensibility.Register(() => new SignalRConfiguration(webApi.Application));

				signalR(configuration);
			});

			return webApi;
		}
	}
}