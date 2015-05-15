using System.Net.Http;

namespace Vertica.Integration.Infrastructure.Remote
{
    public class HttpClientFactory : IHttpClientFactory
    {
        public HttpClient Create()
        {
            return new HttpClient(new HttpClientHandler { UseDefaultCredentials = true });
        }
    }
}