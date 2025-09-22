using FjDailyReport.Models;
using System.Collections.Concurrent;

namespace FjDailyReport.Services;

/// <summary>
/// 简单的内存用户存储服务，实际项目中应该使用数据库
/// </summary>
public class UserService
{
    private readonly ConcurrentDictionary<string, User> _users = new();

    public User GetOrCreateUser(AccessTokenInfo tokenInfo)
    {
        var userId = tokenInfo.Sub;
        
        if (_users.TryGetValue(userId, out var existingUser))
        {
            // 更新最后登录时间
            existingUser.LastLoginAt = DateTime.UtcNow;
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

        _users.TryAdd(userId, newUser);
        return newUser;
    }

    public User? GetUserById(string userId)
    {
        _users.TryGetValue(userId, out var user);
        return user;
    }

    public IEnumerable<User> GetAllUsers()
    {
        return _users.Values.ToList();
    }
}