using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FjDailyReport.Hubs;

[Authorize]
public class DailyReportHub : Hub
{
    /// <summary>
    /// 加入指定日期的房间，以便接收该日期日报的实时更新
    /// </summary>
    public async Task JoinDateRoom(string date)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"date:{date}");
    }

    /// <summary>
    /// 离开指定日期的房间
    /// </summary>
    public async Task LeaveDateRoom(string date)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"date:{date}");
    }
}
