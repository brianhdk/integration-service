using System.Threading.Tasks;

namespace Vertica.Integration.Emails.Mandrill.Infrastructure
{
    public interface IMandrillApiService
    {
        Task<TResponse> RequestAsync<TRequest, TResponse>(string relativeUrl, TRequest request)
            where TRequest : MandrillApiRequest;
    }
}