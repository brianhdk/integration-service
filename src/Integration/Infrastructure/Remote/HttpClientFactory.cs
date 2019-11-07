using System;
using System.Net.Http;

namespace Vertica.Integration.Infrastructure.Remote
{
    public class HttpClientFactory : IHttpClientFactory
    {
        [Obsolete("Will be removed in later versions. Use HttpClient directly - remember to re-use the instance.")]
        public HttpClient Create()
        {
            return new HttpClient(new HttpClientHandler { UseDefaultCredentials = true });
        }
    }
}