using System.Net.Http;

namespace Vertica.Integration.Infrastructure.Remote
{
    public interface IHttpClientFactory
    {
        HttpClient Create();
    }
}