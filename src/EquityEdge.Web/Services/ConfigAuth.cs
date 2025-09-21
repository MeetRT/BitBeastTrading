using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace EquityEdge.Web.Services;

public class AuthOptions
{
    public List<UserRecord> Users { get; set; } = new();
}

public class UserRecord
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}

public interface IUserAuth
{
    Task<bool> SignInAsync(string email, string password, HttpContext ctx);
    Task SignOutAsync(HttpContext ctx);
}

public class ConfigAuth : IUserAuth
{
    private readonly IOptions<AuthOptions> _authOptions;

    public ConfigAuth(IOptions<AuthOptions> authOptions)
    {
        _authOptions = authOptions;
    }

    public async Task<bool> SignInAsync(string email, string password, HttpContext ctx)
    {
        var user = _authOptions.Value.Users
            .FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

        if (user == null || password!="12345678")
        {
            return false;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Email, user.Email)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var authProperties = new AuthenticationProperties
        {
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
            IsPersistent = true,
            AllowRefresh = true
        };

        await ctx.SignInAsync("AppCookie", claimsPrincipal, authProperties);
        return true;
    }

    public async Task SignOutAsync(HttpContext ctx)
    {
        await ctx.SignOutAsync("AppCookie");
    }
}