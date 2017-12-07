using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Castle.MicroKernel;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Emails.Mandrill.Infrastructure
{
    internal class MandrillApiService : IMandrillApiService, IDisposable
    {
        private readonly MandrillSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly IShutdown _shutdown;

        public MandrillApiService(MandrillConfiguration.State state, IKernel kernel, IMandrillSettingsProvider configurationProvider, IShutdown shutdown)
        {
            _shutdown = shutdown;
            _settings = configurationProvider.Get();
            _settings.Validate();

            _httpClient = state.HttpMessageHandler != null
                ? new HttpClient(state.HttpMessageHandler(kernel))
                : new HttpClient();

            _httpClient.BaseAddress = _settings.BaseUrl;
        }

        public async Task RequestAsync<TRequest>(string relativeUrl, TRequest request) where TRequest : MandrillApiRequest
        {
            if (string.IsNullOrWhiteSpace(relativeUrl)) throw new ArgumentException(@"Value cannot be null or empty", nameof(relativeUrl));
            if (request == null) throw new ArgumentNullException(nameof(request));

            request.Key = _settings.ApiKey;

            try
            {
                using (HttpResponseMessage response = await _httpClient.PostAsJsonAsync(relativeUrl, request, _shutdown.Token))
                {
                    await AssertResponse(response);
                }
            }
            catch (WebException ex)
            {
                throw new InvalidOperationException($"Unable to request Mandrill '{relativeUrl}'. Response: {await GetResponseBody(ex)}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to request Mandrill '{relativeUrl}'. Message: {ex.Message}", ex);
            }
        }

        public async Task<TResponse> RequestAsync<TRequest, TResponse>(string relativeUrl, TRequest request)
            where TRequest : MandrillApiRequest
        {
            if (string.IsNullOrWhiteSpace(relativeUrl)) throw new ArgumentException(@"Value cannot be null or empty", nameof(relativeUrl));
            if (request == null) throw new ArgumentNullException(nameof(request));

            request.Key = _settings.ApiKey;

            try
            {
                using (HttpResponseMessage response = await _httpClient.PostAsJsonAsync(relativeUrl, request, _shutdown.Token))
                {
                    await AssertResponse(response);

                    return await response.Content.ReadAsAsync<TResponse>(_shutdown.Token);
                }
            }
            catch (WebException ex)
            {
                throw new InvalidOperationException($"Unable to request Mandrill '{relativeUrl}'. Response: {await GetResponseBody(ex)}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to request Mandrill '{relativeUrl}'. Message: {ex.Message}", ex);
            }
        }

        public async Task RequestAsync<TRequest>(string relativeUrl, TRequest request, Action<HttpResponseMessage> withResponse)
            where TRequest : MandrillApiRequest
        {
            if (string.IsNullOrWhiteSpace(relativeUrl)) throw new ArgumentException(@"Value cannot be null or empty", nameof(relativeUrl));
            if (request == null) throw new ArgumentNullException(nameof(request));

            request.Key = _settings.ApiKey;

            try
            {
                using (HttpResponseMessage response = await _httpClient.PostAsJsonAsync(relativeUrl, request, _shutdown.Token))
                {
                    await AssertResponse(response);

                    withResponse(response);
                }
            }
            catch (WebException ex)
            {
                throw new InvalidOperationException($"Unable to request Mandrill '{relativeUrl}'. Response: {await GetResponseBody(ex)}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to request Mandrill '{relativeUrl}'. Message: {ex.Message}", ex);
            }
        }

        private static async Task AssertResponse(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                string error = await response.Content.ReadAsStringAsync();

                throw new HttpRequestException($"{error ?? "n/a"}. Uri: {response.RequestMessage.RequestUri}");
            }

            response.EnsureSuccessStatusCode();
        }

        private static async Task<string> GetResponseBody(WebException ex)
        {
            Stream stream = ex.Response?.GetResponseStream();

            if (stream == null || stream == Stream.Null)
                return null;

            using (StreamReader streamReader = new StreamReader(stream))
            {
                return await streamReader.ReadToEndAsync();
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}