using Microsoft.Extensions.FileProviders;

namespace FjDailyReport.Infrastructure;

public class FrontendMiddleware(RequestDelegate next, IWebHostEnvironment webHostEnvironment)
{
    private readonly IFileProvider fileProvider = webHostEnvironment.WebRootFileProvider;

    public async Task Invoke(HttpContext context)
    {
        do
        {
            if (ShouldBypassProcessing(context))
            {
                break;
            }

            foreach (string tryPath in EnumerateTryPaths(context.Request.Path))
            {
                IFileInfo fileInfo = fileProvider.GetFileInfo(tryPath);
                if (fileInfo.Exists)
                {
                    context.Request.Path = tryPath;
                    break;
                }
            }
        } while (false);

        await next(context);
    }

    static IEnumerable<string> EnumerateTryPaths(string requestPath)
    {
        // 规范化：始终以"/"开头
        if (string.IsNullOrEmpty(requestPath))
            yield break;

        // 根路径直接回退到根 index.html
        if (requestPath == "/")
        {
            yield return "/index.html";
            yield break;
        }

        // 对于无扩展名的路由（SPA 页面），优先尝试 /path/index.html（配合 Next.js trailingSlash: true）
        if (!Path.HasExtension(requestPath))
        {
            string withSlash = requestPath.EndsWith('/') ? requestPath : requestPath + "/";
            yield return withSlash + "index.html"; // 例如：/login -> /login/index.html

            // 兼容未开启 trailingSlash 的导出：/path.html
            yield return requestPath + ".html"; // 例如：/login -> /login.html
        }

        // 最终兜底到根 index.html（单页应用）
        yield return "/index.html";
    }

    private bool ShouldBypassProcessing(HttpContext context)
    {
        var path = context.Request.Path;
        return path.Value == null ||
            // 后端与文档路由直接跳过
            path.StartsWithSegments("/api") ||
            path.StartsWithSegments("/swagger") ||
            path.StartsWithSegments("/v1") ||
            // Next 静态资源必须原样返回，不能回退到 index.html
            path.StartsWithSegments("/_next") ||
            // 仅处理 GET/HEAD
            !IsGetOrHeadMethod(context.Request.Method) ||
            // 已存在的静态文件直接交给 StaticFiles 处理
            fileProvider.GetFileInfo(path).Exists;
    }

    private static bool IsGetOrHeadMethod(string method) => HttpMethods.IsGet(method) || HttpMethods.IsHead(method);
}