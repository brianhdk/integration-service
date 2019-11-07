using System;
using System.Net.Http;

namespace Vertica.Integration.Infrastructure.Remote
{
    public interface IHttpClientFactory
    {
        [Obsolete("Will be removed in later versions. Use HttpClient directly - remember to re-use the instance.")]
        HttpClient Create();
    }
}