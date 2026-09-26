namespace API.Middleware;

/// <summary>
/// The API returns JSON and raw bytes (for example encrypted secret chat files), never pages. These headers stop a
/// browser from sniffing a response into something it would render or run, and from framing it.
/// Swagger UI is an HTML page with scripts, so it is left alone.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/swagger"))
        {
            var headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
            headers["Referrer-Policy"] = "no-referrer";
        }

        return next(context);
    }
}
