using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FjDailyReport.Models;
using FjDailyReport.Services;
using System.Security.Claims;

namespace FjDailyReport.Controllers;

[Route("api/user")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserInfo>> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(new UserInfo
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName
        });
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<UserInfo>>> GetAllUsers()
    {
        var users = (await _userService.GetAllUsersAsync()).Select(user => new UserInfo
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName
        });

        return Ok(users);
    }
}