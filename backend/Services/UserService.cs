using FjDailyReport.DB;
using FjDailyReport.Models;
using Microsoft.EntityFrameworkCore;

namespace FjDailyReport.Services;

/// <summary>
/// 用户服务，通过数据库管理用户数据
/// </summary>
public class UserService(AppDB db)
{
    public async Task<User> GetOrCreateUserAsync(AccessTokenInfo tokenInfo)
    {
        var userId = tokenInfo.Sub;

        var existingUser = await db.Users.FindAsync(userId);
        if (existingUser != null)
        {
            // 更新最后登录时间
            existingUser.LastLoginAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return existingUser;
        }

        // 创建新用户
        var newUser = new User
        {
            Id = userId,
            Email = tokenInfo.Email ?? "",
            DisplayName = tokenInfo.GetDisplayName(),
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        db.Users.Add(newUser);
        await db.SaveChangesAsync();
        return newUser;
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        return await db.Users.FindAsync(userId);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await db.Users.ToListAsync();
    }
}