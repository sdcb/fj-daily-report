using Microsoft.Extensions.Options;
using FjDailyReport.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace FjDailyReport.Services;

public class KeycloakService
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakConfig _config;

    public KeycloakService(HttpClient httpClient, IOptions<KeycloakConfig> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
    }

    public async Task<string> GenerateLoginUrl(string redirectUrl, CancellationToken cancellationToken = default)
    {
        var config = await LoadWellknown(cancellationToken);
        var scope = "openid";
        var encodedRedirectUrl = Uri.EscapeDataString(redirectUrl);
        return $"{config.AuthorizationEndpoint}?client_id={_config.ClientId}&redirect_uri={encodedRedirectUrl}&response_type=code&scope={scope}";
    }

    public async Task<AccessTokenInfo> GetUserInfo(string code, string redirectUrl, CancellationToken cancellationToken = default)
    {
        var config = await LoadWellknown(cancellationToken);

        var tokenResponse = await _httpClient.PostAsync(config.TokenEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret,
            ["code"] = code,
            ["redirect_uri"] = redirectUrl,
        }), cancellationToken);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            var errorContent = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Failed to get access token: {errorContent}");
        }

        var tokenDto = await tokenResponse.Content.ReadFromJsonAsync<SsoTokenDto>(cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize token response");

        return DecodeAccessToken(tokenDto.AccessToken);
    }

    private async Task<KeycloakOAuthConfig> LoadWellknown(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(_config.WellKnown, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<KeycloakOAuthConfig>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to deserialize well-known configuration");
        }

        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException($"Failed to get Keycloak well-known configuration: {errorContent}");
    }

    private static AccessTokenInfo DecodeAccessToken(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(accessToken);
        
        var claims = jsonToken.Claims.ToDictionary(c => c.Type, c => c.Value);
        
        return new AccessTokenInfo
        {
            Sub = claims.GetValueOrDefault("sub") ?? throw new InvalidOperationException("sub claim not found"),
            GivenName = claims.GetValueOrDefault("given_name"),
            FamilyName = claims.GetValueOrDefault("family_name"),
            Email = claims.GetValueOrDefault("email"),
            PreferredUsername = claims.GetValueOrDefault("preferred_username"),
            Name = claims.GetValueOrDefault("name")
        };
    }
}