using System.Text.Json.Serialization;

namespace FjDailyReport.Models;

public class DailyReportDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("userId")]
    public required string UserId { get; set; }

    [JsonPropertyName("userDisplayName")]
    public required string UserDisplayName { get; set; }

    [JsonPropertyName("date")]
    public required string Date { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

public class UpdateDailyReportRequest
{
    [JsonPropertyName("userId")]
    public required string UserId { get; set; }

    [JsonPropertyName("date")]
    public required string Date { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}

public class ProjectGroupDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("members")]
    public required List<ProjectGroupMemberDto> Members { get; set; }
}

public class ProjectGroupMemberDto
{
    [JsonPropertyName("userId")]
    public required string UserId { get; set; }

    [JsonPropertyName("displayName")]
    public required string DisplayName { get; set; }

    [JsonPropertyName("email")]
    public required string Email { get; set; }
}

public class DailyReportsResponse
{
    [JsonPropertyName("date")]
    public required string Date { get; set; }

    [JsonPropertyName("reports")]
    public required List<DailyReportDto> Reports { get; set; }

    [JsonPropertyName("groups")]
    public required List<ProjectGroupDto> Groups { get; set; }
}
