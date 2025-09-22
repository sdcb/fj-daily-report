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
    public ActionResult<UserInfo> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = _userService.GetUserById(userId);
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
    public ActionResult<IEnumerable<UserInfo>> GetAllUsers()
    {
        var users = _userService.GetAllUsers().Select(user => new UserInfo
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName
        });

        return Ok(users);
    }
}