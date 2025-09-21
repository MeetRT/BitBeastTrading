using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EquityEdge.Web.Services;

public static class KiteEndpoints
{
    private static readonly Dictionary<string, string> _userTokens = new();

    public static void MapKiteEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/kite");

        group.MapGet("/login", [Authorize] (HttpContext ctx, KiteClient kiteClient) =>
        {
            var state = Guid.NewGuid().ToString();
            var loginUrl = kiteClient.GetLoginUrl(state);
            return Results.Redirect(loginUrl);
        });

        group.MapGet("/callback", [Authorize] async (
            string? request_token,
            string? status,
            HttpContext ctx,
            KiteClient kiteClient) =>
        {
            if (string.IsNullOrEmpty(request_token) || status != "success")
            {
                return Results.Redirect("/settings?error=auth_failed");
            }

            try
            {
                var accessToken = await kiteClient.ExchangeRequestToken(request_token);
                var userEmail = ctx.User.FindFirst(ClaimTypes.Email)?.Value ?? "";
                
                if (!string.IsNullOrEmpty(userEmail))
                {
                    _userTokens[userEmail] = accessToken;
                }

                return Results.Redirect("/settings?success=true");
            }
            catch
            {
                return Results.Redirect("/settings?error=token_exchange_failed");
            }
        });

        group.MapGet("/me", [Authorize] (HttpContext ctx) =>
        {
            var userEmail = ctx.User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var linked = !string.IsNullOrEmpty(userEmail) && _userTokens.ContainsKey(userEmail);
            
            return Results.Ok(new { email = userEmail, linked });
        });

        group.MapGet("/token", [Authorize] (HttpContext ctx) =>
        {
            var userEmail = ctx.User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            
            if (string.IsNullOrEmpty(userEmail) || !_userTokens.TryGetValue(userEmail, out var token))
            {
                return Results.NotFound();
            }

            return Results.Ok(new { token });
        });
    }
}