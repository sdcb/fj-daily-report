namespace FjDailyReport.DB;

public class ProjectGroupMember
{
    public int ProjectGroupId { get; set; }
    public required string UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    public ProjectGroup ProjectGroup { get; set; } = null!;
    public User User { get; set; } = null!;
}
