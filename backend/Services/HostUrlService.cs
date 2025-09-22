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
        
        // 首先尝试从Referer头获取，如果没有则使用配置文件中的FE_URL
        if (headers.TryGetValue("Referer", out StringValues refererValue))
        {
            var refererUrl = refererValue.FirstOrDefault();
            if (!string.IsNullOrEmpty(refererUrl))
            {
                // 从Referer URL中提取基础URL (scheme + host)
                var uri = new Uri(refererUrl);
                return $"{uri.Scheme}://{uri.Host}{(uri.Port != 80 && uri.Port != 443 ? $":{uri.Port}" : "")}";
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
}