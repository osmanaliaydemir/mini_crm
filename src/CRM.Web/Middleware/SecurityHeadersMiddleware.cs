namespace CRM.Web.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var response = context.Response;

        // X-Frame-Options: Clickjacking koruması
        response.Headers.Append("X-Frame-Options", "DENY");

        // X-Content-Type-Options: MIME type sniffing koruması
        response.Headers.Append("X-Content-Type-Options", "nosniff");

        // Referrer-Policy: Referrer bilgisi kontrolü
        response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Permissions-Policy: Özellik izinleri
        response.Headers.Append("Permissions-Policy",
            "geolocation=(), microphone=(), camera=(), payment=(), usb=(), magnetometer=(), gyroscope=(), accelerometer=()");

        // Content-Security-Policy: XSS ve injection koruması
        var csp = BuildContentSecurityPolicy();
        response.Headers.Append("Content-Security-Policy", csp);

        // X-XSS-Protection: Eski tarayıcılar için (modern tarayıcılar CSP kullanır)
        response.Headers.Append("X-XSS-Protection", "1; mode=block");

        await _next(context);
    }

    private string BuildContentSecurityPolicy()
    {
        // Development ortamında daha esnek CSP, production'da sıkı
        if (_environment.IsDevelopment())
        {
            return "default-src 'self'; " +
                   "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.datatables.net https://cdn.jsdelivr.net; " +
                   "style-src 'self' 'unsafe-inline' https://cdn.datatables.net https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
                   "img-src 'self' data: https:; " +
                   "font-src 'self' data: https://cdn.jsdelivr.net https://fonts.gstatic.com; " +
                   "connect-src 'self' https://cdn.datatables.net https://cdn.jsdelivr.net ws://localhost:* wss://localhost:* http://localhost:*; " +
                   "frame-ancestors 'none'; " +
                   "base-uri 'self'; " +
                   "form-action 'self'; " +
                   "upgrade-insecure-requests;";
        }

        // Production CSP - Daha sıkı
        return "default-src 'self'; " +
               "script-src 'self' https://cdn.datatables.net https://cdn.jsdelivr.net; " +
               "style-src 'self' 'unsafe-inline' https://cdn.datatables.net https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
               "img-src 'self' data: https:; " +
               "font-src 'self' data: https://cdn.jsdelivr.net https://fonts.gstatic.com; " +
               "connect-src 'self' https://cdn.datatables.net https://cdn.jsdelivr.net; " +
               "frame-ancestors 'none'; " +
               "base-uri 'self'; " +
               "form-action 'self'; " +
               "upgrade-insecure-requests;";
    }
}

