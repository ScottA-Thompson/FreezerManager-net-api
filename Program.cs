using FreezerManager.Data;
using Microsoft.EntityFrameworkCore;
using FreezerManager.Endpoints;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using FreezerManager.Services;
using AuthenticationMiddleware = FreezerManager.AuthenticationMiddleware;
using AuthenticationService = FreezerManager.Services.AuthenticationService;
using IAuthenticationService = Microsoft.AspNetCore.Authentication.IAuthenticationService;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.UseUrls("http://0.0.0.0:5123");
builder.Services.AddDbContext<AppDbContext>(options => 
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRazorPages();
builder.Services.AddScoped<FreezerManager.Services.IAuthenticationService, AuthenticationService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);     
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //db.Database.Migrate(); 
    db.Database.EnsureCreated();
}

app.UseDefaultFiles();
app.UseSession();
app.UseStaticFiles();
app.UseMiddleware<AuthenticationMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapAuthEndpoints();
app.MapRazorPages();
app.MapMeatItemEndpoints();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapStaticAssets();

app.Run();

public record LoginRequest(string Pin);
