using System;
using System.Net;
using System.Text;
using Castle.MicroKernel;

namespace Vertica.Integration.Perfion
{
	public class WebClientConfiguration
	{
		internal Action<IKernel, WebClient> Configuration { get; private set; }

		public WebClientConfiguration Configure(Action<IKernel, WebClient> configuration)
		{
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));

			Configuration = configuration;

			return this;
		}

		public void SetBasicAuthentication(WebClient webClient, string username, string password)
		{
			if (webClient == null) throw new ArgumentNullException(nameof(webClient));
			if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException(@"Value cannot be null or empty", nameof(username));

			string format = $"{username}:{password}";

			string credentialsEncoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(format));

			webClient.Headers[HttpRequestHeader.Authorization] = $"Basic {credentialsEncoded}";
		}
	}
}