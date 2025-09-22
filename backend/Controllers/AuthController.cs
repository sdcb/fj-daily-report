using Microsoft.AspNetCore.Mvc;
using FjDailyReport.Models;
using FjDailyReport.Services;

namespace FjDailyReport.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly KeycloakService _keycloakService;
    private readonly UserService _userService;
    private readonly JwtService _jwtService;
    private readonly HostUrlService _hostUrlService;

    public AuthController(
        KeycloakService keycloakService,
        UserService userService,
        JwtService jwtService,
        HostUrlService hostUrlService)
    {
        _keycloakService = keycloakService;
        _userService = userService;
        _jwtService = jwtService;
        _hostUrlService = hostUrlService;
    }

    [HttpGet("keycloak/signin")]
    public async Task<IActionResult> KeycloakSignIn([FromQuery] string? origin, CancellationToken cancellationToken)
    {
        var callbackUrl = _hostUrlService.GetKeycloakSsoRedirectUrl(origin ?? string.Empty);
        var keycloakUrl = await _keycloakService.GenerateLoginUrl(callbackUrl, cancellationToken);
        return Redirect(keycloakUrl);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (request.Provider?.Equals("Keycloak", StringComparison.OrdinalIgnoreCase) == true && !string.IsNullOrEmpty(request.Code))
        {
            var redirectUrl = _hostUrlService.GetKeycloakSsoRedirectUrl(request.Origin ?? string.Empty);
            var tokenInfo = await _keycloakService.GetUserInfo(request.Code, redirectUrl, cancellationToken);
            var user = _userService.GetOrCreateUser(tokenInfo);
            var token = _jwtService.GenerateToken(user);

            return Ok(new LoginResponse
            {
                Token = token,
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    DisplayName = user.DisplayName
                }
            });
        }

        return BadRequest("Invalid login request");
    }
}