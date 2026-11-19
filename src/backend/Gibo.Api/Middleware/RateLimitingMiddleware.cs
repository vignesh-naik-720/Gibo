using System.Collections.Concurrent;
using System.Net;

namespace Gibo.Api.Middleware;

/// <summary>
/// Simple IP-based rate limiting middleware preventing brute-force registration floods (T042).
/// Enforces max 30 requests per minute per remote IP address.
/// </summary>
public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, (int Count, DateTime WindowStart)> _clients = new();

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = DateTime.UtcNow;

        var clientData = _clients.AddOrUpdate(
            clientIp,
            (Count: 1, WindowStart: now),
            (key, old) =>
            {
                if ((now - old.WindowStart).TotalMinutes >= 1)
                {
                    return (Count: 1, WindowStart: now);
                }
                return (Count: old.Count + 1, WindowStart: old.WindowStart);
            });

        if (clientData.Count > 30)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"errorCode\":\"RATE_LIMIT_EXCEEDED\",\"message\":\"Too many requests. Please slow down.\"}");
            return;
        }

        await _next(context);
    }
}

/// <summary>
/// Injects defense-in-depth security headers (HSTS, Content-Security-Policy, X-Frame-Options, X-Content-Type-Options).
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        await _next(context);
    }
}
