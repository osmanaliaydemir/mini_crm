using System.Globalization;
using CRM.Application;
using CRM.Infrastructure;
using CRM.Infrastructure.Persistence;
using CRM.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using CRM.Web;
using CRM.Web.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using CRM.Web.Localization;
using Quartz;
using Serilog;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Serilog yapılandırması - appsettings.json'dan oku
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

try
{
    Log.Information("Starting CRM Web Application");

    // Serilog'u ASP.NET Core logging provider olarak ekle
    builder.Host.UseSerilog();

// Memory Cache
builder.Services.AddMemoryCache();

// Response Caching - Static content and API responses
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 64 * 1024 * 1024; // 64 MB
    options.UseCaseSensitivePaths = false;
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SignInManager<ApplicationUser>>();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>>();

builder.Services.AddQuartz();
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddLocalization();
builder.Services.Configure<JsonLocalizationOptions>(options =>
{
    options.ResourcesPath = "Resources";
    options.FallBackToParentCultures = true;
});
builder.Services.AddSingleton<IFileProvider>(sp => sp.GetRequiredService<IHostEnvironment>().ContentRootFileProvider);
builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
builder.Services.AddSingleton(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // 8 saat oturum süresi
        options.Cookie.Name = "__crm_portal";
        
        // Cookie Security - OWASP Best Practices
        options.Cookie.HttpOnly = true; // JavaScript'ten erişilemez
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS'de Secure
        options.Cookie.SameSite = SameSiteMode.Strict; // CSRF koruması
    });

const string AdminRole = "Admin";
const string PersonnelRole = "Personel";

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(AdminRole));
    options.AddPolicy("FinanceAccess", policy =>
        policy.RequireRole(AdminRole));
    options.AddPolicy("OperationsAccess", policy =>
        policy.RequireRole(AdminRole, PersonnelRole));
});

builder.Services.AddRazorPages(options =>
    {
        options.Conventions.AllowAnonymousToPage("/Auth/Login");
        options.Conventions.AllowAnonymousToPage("/Auth/ForgotPassword");
        // Error pages should be accessible to everyone
        options.Conventions.AllowAnonymousToPage("/Error/404");
        options.Conventions.AllowAnonymousToPage("/Error/403");
        options.Conventions.AllowAnonymousToPage("/Error/500");
        options.Conventions.AuthorizeFolder("/");
        options.Conventions.AuthorizeFolder("/Suppliers", "OperationsAccess");
        options.Conventions.AuthorizeFolder("/Warehouses", "OperationsAccess");
        options.Conventions.AuthorizeFolder("/Customers", "OperationsAccess");
        options.Conventions.AuthorizeFolder("/Finance", "FinanceAccess");
        options.Conventions.AuthorizeFolder("/Users", "AdminOnly");
        options.Conventions.AuthorizeFolder("/Settings", "AdminOnly");
        options.Conventions.AuthorizeFolder("/Tasks", "OperationsAccess");
        options.Conventions.AuthorizePage("/Index", "OperationsAccess");
    })
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (_, factory) =>
            factory.Create(typeof(SharedResource));
    });

// FluentValidation - Lokalize validation desteği
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

var supportedCultures = new[]
{
    new CultureInfo("tr-TR"),
    new CultureInfo("en-US"),
    new CultureInfo("ar-SA")
};
var supportedCultureNames = supportedCultures.Select(c => c.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
var defaultCultureName = supportedCultures[0].Name;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("tr-TR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
    options.RequestCultureProviders.Insert(1, new CookieRequestCultureProvider());
});

var app = builder.Build();

IResult HandleCultureChange(HttpContext context, string? culture, string? returnUrl)
{
    var targetCulture = string.IsNullOrWhiteSpace(culture) ? defaultCultureName : culture;
    if (!supportedCultureNames.Contains(targetCulture))
    {
        targetCulture = defaultCultureName;
    }

    var requestCulture = new RequestCulture(targetCulture);
    var cookieValue = CookieRequestCultureProvider.MakeCookieValue(requestCulture);

    context.Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        cookieValue,
        new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            IsEssential = true,
            Path = "/"
        });

    var redirectUrl = string.IsNullOrWhiteSpace(returnUrl) || !Uri.IsWellFormedUriString(returnUrl, UriKind.Relative)
        ? "/"
        : returnUrl;

    return Results.Redirect(redirectUrl);
}

await using (var scope = app.Services.CreateAsyncScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.InitializeAsync();
}

var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

// Security Headers Middleware - Güvenlik başlıklarını ekle
app.UseSecurityHeaders();

// Response Caching - Static content caching
app.UseResponseCaching();

// Status Code Pages - Custom error pages için
app.UseStatusCodePagesWithReExecute("/Error/{0}");

// Global Exception Handler Middleware - Tüm exception'ları yakalar
app.UseGlobalExceptionHandler();

// Rate Limiting Middleware - Brute force ve DDoS koruması
app.UseRateLimiting(options =>
{
    options.MaxRequests = 10; // Login ve API için 10 istek
    options.Window = TimeSpan.FromMinutes(1); // 1 dakika içinde
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/culture/set", (HttpContext context, string culture, string? returnUrl) =>
    HandleCultureChange(context, culture, returnUrl));

app.MapPost("/culture/set", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var culture = form["culture"].ToString();
    var returnUrl = form["returnUrl"].ToString();
    return HandleCultureChange(context, culture, returnUrl);
});

app.MapRazorPages();

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Integration testler için Program class'ını public yapıyoruz
public partial class Program { }