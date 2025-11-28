namespace FjDailyReport.DB;

public class User
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    public List<DailyReport> DailyReports { get; set; } = [];
    public List<ProjectGroupMember> ProjectGroupMembers { get; set; } = [];
}
