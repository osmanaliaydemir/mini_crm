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
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using CRM.Web.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SignInManager<ApplicationUser>>();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>>();

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
        options.Cookie.Name = "__crm_portal";
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
        options.Conventions.AuthorizeFolder("/");
        options.Conventions.AuthorizeFolder("/Suppliers", "OperationsAccess");
        options.Conventions.AuthorizeFolder("/Warehouses", "OperationsAccess");
        options.Conventions.AuthorizeFolder("/Customers", "OperationsAccess");
        options.Conventions.AuthorizeFolder("/Finance", "FinanceAccess");
        options.Conventions.AuthorizePage("/Index", "OperationsAccess");
    })
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (_, factory) =>
            factory.Create(typeof(SharedResource));
    });

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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
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
