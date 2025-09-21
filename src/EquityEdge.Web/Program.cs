using Microsoft.AspNetCore.Authentication.Cookies;
using EquityEdge.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// Configure authentication
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));

builder.Services.AddAuthentication("AppCookie")
    .AddCookie("AppCookie", options =>
    {
        options.Cookie.Name = "ee.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/denied";
    });

builder.Services.AddAuthorization();

// Register singletons
builder.Services.AddSingleton<IUserAuth, ConfigAuth>();
builder.Services.AddSingleton<KiteClient>();
builder.Services.AddSingleton<LiveTickerService>();
builder.Services.AddSingleton<BarBuilderService>();
builder.Services.AddSingleton<WatchlistStore>();
builder.Services.AddSingleton<BacktestService>();
builder.Services.AddSingleton<LiveTickerBridge>();
builder.Services.AddSingleton<LiveInterop>();

// Add hosted service
builder.Services.AddHostedService<StartupWiring>();

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Map endpoints
app.MapAuthEndpoints();
app.MapKiteEndpoints();

app.Run();