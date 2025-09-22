using Microsoft.Extensions.Primitives;

namespace FjDailyReport.Services;

public class HostUrlService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public HostUrlService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public virtual string GetBEUrl()
    {
        var request = _httpContextAccessor.HttpContext!.Request;
        var headers = request.Headers;

        var scheme = headers.TryGetValue("X-Forwarded-Proto", out StringValues schemeValue) ? schemeValue.FirstOrDefault()! : request.Scheme;
        var host = headers.TryGetValue("X-Forwarded-Host", out StringValues hostValue) ? hostValue.FirstOrDefault()! : request.Host.ToString();

        return $"{scheme}://{host}";
    }

    public virtual string GetFEUrl()
    {
        var request = _httpContextAccessor.HttpContext!.Request;
        var headers = request.Headers;
        
        // 首先尝试从Origin头获取，如果没有则使用配置文件中的FE_URL
        if (headers.TryGetValue("Origin", out StringValues originValue))
        {
            var originUrl = originValue.FirstOrDefault();
            if (!string.IsNullOrEmpty(originUrl))
            {
                // Origin头直接包含基础URL (scheme + host + port)
                return originUrl;
            }
        }
        
        // Fallback到配置文件中的前端URL
        var feUrl = _configuration["FE_URL"];
        if (string.IsNullOrEmpty(feUrl))
        {
            throw new InvalidOperationException("Both Referer header and FE_URL configuration are missing");
        }
        
        return feUrl;
    }

    public virtual string GetKeycloakSsoRedirectUrl()
    {
        return $"{GetFEUrl()}/authorizing?provider=Keycloak";
    }

    public virtual string GetKeycloakSsoRedirectUrl(string origin)
    {
        if (string.IsNullOrWhiteSpace(origin))
        {
            // fallback to original behavior if origin not supplied
            return GetKeycloakSsoRedirectUrl();
        }
        return $"{origin}/authorizing?provider=Keycloak";
    }
}