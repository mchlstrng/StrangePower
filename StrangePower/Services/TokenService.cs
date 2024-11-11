using StrangePower.Data;
using System.Net.Http.Headers;

namespace StrangePower.Services;

/// <summary>
/// Represents a service for retrieving access tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Retrieves an access token asynchronously.
    /// </summary>
    /// <returns>The access token.</returns>
    Task<string> GetAccessTokenAsync();
}

/// <summary>
/// Implementation of <see cref="ITokenService"/> for retrieving access tokens.
/// </summary>
public class TokenService : ITokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenRepository _tokenRepository;
    private readonly IConfiguration _configuration;

    public TokenService(IHttpClientFactory httpClientFactory, ITokenRepository tokenRepository, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc/>
    public async Task<string> GetAccessTokenAsync()
    {
        var token = await _tokenRepository.GetTokenAsync();
        if (token == null || token.Expiry < DateTime.UtcNow)
        {
            token = await GetNewAccessToken();
        }
        return token.AccessToken;
    }

    /// <summary>
    /// Retrieves a new access token using the refresh token.
    /// </summary>
    /// <returns>The new access token.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no refresh token is available or the token retrieval fails.</exception>
    private async Task<Token> GetNewAccessToken()
    {
        var refreshToken = _configuration["Authentication:RefreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new InvalidOperationException("No refresh token available.");
        }

        var httpClient = _httpClientFactory.CreateClient("EloverblikClient");
        var request = new HttpRequestMessage(HttpMethod.Get, "/customerapi/api/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshToken);

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var tokenJsonResult = await response.Content.ReadFromJsonAsync<TokenJsonResult>();

        if (tokenJsonResult == null || string.IsNullOrEmpty(tokenJsonResult.Result))
        {
            throw new InvalidOperationException("Failed to retrieve a valid access token.");
        }

        var newToken = new Token
        {
            Expiry = DateTime.UtcNow.AddHours(24),
            AccessToken = tokenJsonResult.Result
        };

        await _tokenRepository.UpdateTokenAsync(newToken);

        return newToken;
    }
}

public class TokenJsonResult
{
    public string Result { get; set; }
}