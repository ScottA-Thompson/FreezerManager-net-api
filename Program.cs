using FreezerManager.Data;
using Microsoft.EntityFrameworkCore;
using FreezerManager.Endpoints;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.UseUrls("http://0.0.0.0:5123");
// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options => 
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRazorPages();

//Add services for auth
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24); //lets see how 24 hours works       
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // Applies any pending migrations
}

//app.UsePathBase("/freezer");
app.UseDefaultFiles();
app.UseSession();
app.UseStaticFiles();

app.Use(async (context, next) => 
{
    var path = context.Request.Path.Value?.ToLower();

    if(path == "/api/auth/login" || path == "//" || path.StartsWith("//css") || path.StartsWith("//js"))
    {
        await next();
        return;
    }

    if (context.Session.GetString("authenticated") != "true")
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }

    await next();
});

app.MapPost("/api/auth/login", async (LoginRequest request, HttpContext context, IConfiguration config) =>
{
    var configuredPin = config["Auth:PIN"];
    var configuredPinHash = config["Auth:PINHash"];
    
    if (string.IsNullOrEmpty(configuredPin) && string.IsNullOrEmpty(configuredPinHash))
    {
        return Results.Problem("PIN not configured");
    }
    
    bool pinValid = false;
    
    // Check against plain PIN (for initial setup)
    if (!string.IsNullOrEmpty(configuredPin))
    {
        pinValid = request.Pin == configuredPin;
    }
    // Check against hashed PIN
    else if (!string.IsNullOrEmpty(configuredPinHash))
    {
        pinValid = VerifyPin(request.Pin, configuredPinHash);
    }
    
    if (!pinValid)
    {
        return Results.Unauthorized();
    }
    
    // Set session
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


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapMeatItemEndpoints();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapStaticAssets();

app.Run();

static string HashPin(string pin)
{
    using var sha256 = SHA256.Create();
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(pin + "FreezerSalt"));
    return Convert.ToBase64String(hashedBytes);
}

static bool VerifyPin(string pin, string hash)
{
    var pinHash = HashPin(pin);
    return pinHash == hash;
}

public record LoginRequest(string Pin);
