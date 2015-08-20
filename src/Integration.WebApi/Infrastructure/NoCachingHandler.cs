using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Vertica.Integration.WebApi.Infrastructure
{
	internal class NoCachingHandler : DelegatingHandler
    {
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			HttpResponseMessage response = base.SendAsync(request, cancellationToken).Result;

			if (response.Headers.CacheControl == null)
			{
				response.Headers.CacheControl = CacheControlHeaderValue.Parse("no-cache, no-store");
				response.Headers.Pragma.ParseAdd("no-cache");
			}

			return Task.FromResult(response);
		}
    }
}