using Microsoft.AspNetCore.Mvc;

namespace EquityEdge.Web.Services;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/auth/login", async (
            [FromBody] LoginRequest request,
            IUserAuth userAuth,
            HttpContext ctx) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { error = "Email and password are required" });
            }

            var success = await userAuth.SignInAsync(request.Email, request.Password, ctx);
            
            if (success)
            {
                return Results.Ok(new { success = true });
            }
            else
            {
                return Results.BadRequest(new { error = "Invalid email or password" });
            }
        });

        endpoints.MapPost("/auth/logout", async (IUserAuth userAuth, HttpContext ctx) =>
        {
            await userAuth.SignOutAsync(ctx);
            return Results.Ok(new { success = true });
        });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}