using System.Net.Http.Headers;

namespace StrangePower.Services
{
    public interface IHttpClientTokenService
    {
        Task<HttpClient> GetHttpClientWithTokenAsync();
    }

    public class HttpClientTokenService : IHttpClientTokenService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenService _tokenService;

        public HttpClientTokenService(IHttpClientFactory httpClientFactory, ITokenService tokenService)
        {
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
        }

        public async Task<HttpClient> GetHttpClientWithTokenAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("EloverblikClient");
            var accessToken = await _tokenService.GetAccessTokenAsync();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return httpClient;
        }
    }
}