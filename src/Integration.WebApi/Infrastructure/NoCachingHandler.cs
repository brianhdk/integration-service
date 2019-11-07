using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Vertica.Integration.WebApi.Infrastructure
{
	internal class NoCachingHandler : DelegatingHandler
    {
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

			if (response.Headers.CacheControl == null)
			{
				response.Headers.CacheControl = CacheControlHeaderValue.Parse("no-cache, no-store");
				response.Headers.Pragma.ParseAdd("no-cache");
			}

			return response;
		}
    }
}