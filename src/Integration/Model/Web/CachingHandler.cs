using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Response=System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage>;

namespace Vertica.Integration.Model.Web
{
    internal class CachingHandler : DelegatingHandler
    {
        protected override async Response SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
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