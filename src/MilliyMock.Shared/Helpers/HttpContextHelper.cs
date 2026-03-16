using Microsoft.AspNetCore.Http;

namespace MilliyMock.Shared.Helpers;

public static class HttpContextHelper
{
    public static IHttpContextAccessor Accessor { get; set; } = null!;

    public static HttpContext? HttpContext => Accessor.HttpContext;

    // ===================== USER =====================
    public static long? UserId
    {
        get
        {
            if (long.TryParse(
                    HttpContext?.User?.FindFirst("id")?.Value,
                    out var userId))
            {
                return userId;
            }

            return null;
        }
    }

    public static string? UserRole =>
        HttpContext?.User?.FindFirst("role")?.Value;

    // ===================== NETWORK =====================
    public static string IpAddress
    {
        get
        {
            // Support reverse proxies (nginx, cloudflare, etc.)
            var forwarded = HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwarded))
                return forwarded.Split(',')[0];

            return HttpContext?.Connection.RemoteIpAddress?.ToString()
                   ?? "unknown";
        }
    }
    
    public static string UserAgent =>
        (HttpContext?.Request.Headers["User-Agent"].ToString() ?? "unknown")
        is var ua && ua.Length > 256 ? ua[..256] : ua;        

    // ===================== REQUEST =====================
    public static string Method =>
        HttpContext?.Request.Method ?? "unknown";

    public static string Path =>
        HttpContext?.Request.Path.Value ?? "unknown";

    public static string FullPath =>
        $"{Method} {Path}";

    // ===================== HEADERS =====================
    public static IHeaderDictionary? RequestHeaders =>
        HttpContext?.Request.Headers;

    public static IHeaderDictionary? ResponseHeaders =>
        HttpContext?.Response.Headers;
}
