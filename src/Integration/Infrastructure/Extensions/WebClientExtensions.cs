using System;
using System.Net;
using System.Text;

namespace Vertica.Integration.Infrastructure.Extensions
{
	public static class WebClientExtensions
	{
		public static void SetBasicAuthentication(this WebClient webClient, string username, string password)
		{
			if (webClient == null) throw new ArgumentNullException(nameof(webClient));
			if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException(@"Value cannot be null or empty", nameof(username));

			string format = $"{username}:{password}";

			string credentialsEncoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(format));

			webClient.Headers[HttpRequestHeader.Authorization] = $"Basic {credentialsEncoded}";
		}
	}
}