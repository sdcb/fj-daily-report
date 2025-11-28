using FjDailyReport.DB;
using FjDailyReport.Hubs;
using FjDailyReport.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FjDailyReport.Controllers;

[Route("api/daily-report")]
[ApiController]
[Authorize]
public class DailyReportController(AppDB db, IHubContext<DailyReportHub> hubContext) : ControllerBase
{
    /// <summary>
    /// 获取指定日期的所有日报和项目组信息
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<DailyReportsResponse>> GetDailyReports([FromQuery] string? date)
    {
        var targetDate = string.IsNullOrEmpty(date) 
            ? DateOnly.FromDateTime(DateTime.Today) 
            : DateOnly.Parse(date);

        // 获取所有项目组及其成员
        var groups = await db.ProjectGroups
            .Include(g => g.Members)
            .ThenInclude(m => m.User)
            .OrderBy(g => g.Id)
            .Select(g => new ProjectGroupDto
            {
                Id = g.Id,
                Name = g.Name,
                Members = g.Members.Select(m => new ProjectGroupMemberDto
                {
                    UserId = m.UserId,
                    DisplayName = m.User.DisplayName,
                    Email = m.User.Email
                }).ToList()
            })
            .ToListAsync();

        // 获取指定日期的所有日报
        var reports = await db.DailyReports
            .Include(r => r.User)
            .Where(r => r.Date == targetDate)
            .Select(r => new DailyReportDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserDisplayName = r.User.DisplayName,
                Date = r.Date.ToString("yyyy-MM-dd"),
                Content = r.Content,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync();

        return Ok(new DailyReportsResponse
        {
            Date = targetDate.ToString("yyyy-MM-dd"),
            Reports = reports,
            Groups = groups
        });
    }

    /// <summary>
    /// 更新或创建日报
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DailyReportDto>> UpdateDailyReport([FromBody] UpdateDailyReportRequest request)
    {
        var targetDate = DateOnly.Parse(request.Date);

        // 查找是否已存在该用户该日期的日报
        var existingReport = await db.DailyReports
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.UserId == request.UserId && r.Date == targetDate);

        DailyReportDto reportDto;

        if (existingReport != null)
        {
            // 更新现有日报
            existingReport.Content = request.Content;
            existingReport.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            reportDto = new DailyReportDto
            {
                Id = existingReport.Id,
                UserId = existingReport.UserId,
                UserDisplayName = existingReport.User.DisplayName,
                Date = existingReport.Date.ToString("yyyy-MM-dd"),
                Content = existingReport.Content,
                UpdatedAt = existingReport.UpdatedAt
            };
        }
        else
        {
            // 验证用户是否存在
            var user = await db.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return NotFound($"User {request.UserId} not found");
            }

            // 创建新日报
            var newReport = new DailyReport
            {
                UserId = request.UserId,
                Date = targetDate,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.DailyReports.Add(newReport);
            await db.SaveChangesAsync();

            reportDto = new DailyReportDto
            {
                Id = newReport.Id,
                UserId = newReport.UserId,
                UserDisplayName = user.DisplayName,
                Date = newReport.Date.ToString("yyyy-MM-dd"),
                Content = newReport.Content,
                UpdatedAt = newReport.UpdatedAt
            };
        }

        // 通过 SignalR 广播更新给同一日期房间的所有用户
        await hubContext.Clients.Group($"date:{request.Date}").SendAsync("ReportUpdated", reportDto);

        return Ok(reportDto);
    }
}
