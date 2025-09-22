using System.Text.Json.Serialization;

namespace FjDailyReport.Models;

public class KeycloakConfig
{
    public required string WellKnown { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
}

public class KeycloakOAuthConfig
{
    [JsonPropertyName("issuer")]
    public required string Issuer { get; set; }

    [JsonPropertyName("authorization_endpoint")]
    public required string AuthorizationEndpoint { get; set; }

    [JsonPropertyName("token_endpoint")]
    public required string TokenEndpoint { get; set; }

    [JsonPropertyName("userinfo_endpoint")]
    public string? UserinfoEndpoint { get; set; }

    [JsonPropertyName("jwks_uri")]
    public string? JwksUri { get; set; }

    [JsonPropertyName("end_session_endpoint")]
    public string? EndSessionEndpoint { get; set; }
}

public class SsoTokenDto
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
}

public class AccessTokenInfo
{
    [JsonPropertyName("sub")]
    public required string Sub { get; set; }

    [JsonPropertyName("given_name")]
    public string? GivenName { get; set; }

    [JsonPropertyName("family_name")]
    public string? FamilyName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("preferred_username")]
    public string? PreferredUsername { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    public string GetDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(FamilyName) && !string.IsNullOrWhiteSpace(GivenName))
            return $"{FamilyName}{GivenName}";

        if (!string.IsNullOrWhiteSpace(PreferredUsername))
            return PreferredUsername;

        if (!string.IsNullOrWhiteSpace(Name))
            return Name;

        if (!string.IsNullOrWhiteSpace(Email))
        {
            var emailParts = Email.Split('@');
            if (emailParts.Length > 0)
                return emailParts[0];
        }

        return Sub;
    }
}