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
        if (requestPath.EndsWith('/'))
        {
            yield return requestPath + "index.html"; // example: /login -> /login/index.html
        }

        if (!Path.HasExtension(requestPath))
        {
            yield return requestPath + ".html"; // example: /login -> /login.html

            int lastIndexOfSlash = requestPath.LastIndexOf('/');
            if (lastIndexOfSlash != -1)
            {
                string prefixPart = requestPath[..lastIndexOfSlash];
                yield return prefixPart + ".html"; // example: /login/ -> /login.html
                yield return prefixPart + "/[id].html";
            }
        }

        // 如果都没找到，尝试返回根目录的 index.html（用于 SPA 应用）
        yield return "/index.html";
    }

    private bool ShouldBypassProcessing(HttpContext context)
    {
        return context.Request.Path.Value == null ||
            context.Request.Path.StartsWithSegments("/api") ||
            context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/v1") ||
            !IsGetOrHeadMethod(context.Request.Method) ||
            fileProvider.GetFileInfo(context.Request.Path).Exists;
    }

    private static bool IsGetOrHeadMethod(string method) => HttpMethods.IsGet(method) || HttpMethods.IsHead(method);
}