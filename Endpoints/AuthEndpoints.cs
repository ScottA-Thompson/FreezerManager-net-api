using FreezerManager.Services;
namespace FreezerManager.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/login", async (LoginRequest request, HttpContext context, IAuthenticationService authService) =>
        {
            if (!authService.ValidatePin(request.Pin))
            {
                return Results.Unauthorized();
            }

            context.Session.SetString("authenticated", "true");
            return Results.Ok(new { success = true, message = "Login successful" });
        });

        app.MapPost("/api/auth/logout", (HttpContext context) =>
        {
            context.Session.Clear();
            return Results.Ok(new { success = true, message = "Logged out" });
        });

        app.MapGet("/api/auth/status", (HttpContext context) =>
        {
            var isAuthenticated = context.Session.GetString("authenticated") == "true";
            return Results.Ok(new { authenticated = isAuthenticated });
        });
    } 
}