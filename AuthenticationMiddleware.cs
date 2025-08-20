namespace FreezerManager;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string[] _publicPaths = { "/api/auth/login", "/", "/css", "/js" };

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
        
        if (_publicPaths.Any(p => path.StartsWith(p)))
        {
            await _next(context);
            return;
        }

        if (context.Session.GetString("authenticated") != "true")
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        await _next(context);
    } 
}