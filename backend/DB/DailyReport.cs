namespace FjDailyReport.DB;

public class DailyReport
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public DateOnly Date { get; set; }
    public required string Content { get; set; }
    /// <summary>
    /// 请假状态: null=不请假, "off"=全天请假, "AM leave"=上午请假, "PM leave"=下午请假
    /// </summary>
    public string? LeaveStatus { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    public User User { get; set; } = null!;
}
