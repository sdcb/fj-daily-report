using System.Text.Json.Serialization;

namespace FjDailyReport.Models;

public class LoginRequest
{
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}

public class LoginResponse
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }

    [JsonPropertyName("user")]
    public required UserInfo User { get; set; }
}

public class UserInfo
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("email")]
    public required string Email { get; set; }

    [JsonPropertyName("displayName")]
    public required string DisplayName { get; set; }
}