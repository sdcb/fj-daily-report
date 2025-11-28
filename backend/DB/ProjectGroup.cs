namespace FjDailyReport.DB;

public class ProjectGroup
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    public List<ProjectGroupMember> Members { get; set; } = [];
}
