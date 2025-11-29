namespace FjDailyReport.DB;

public class DailyReport
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public DateOnly Date { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    public User User { get; set; } = null!;
}
