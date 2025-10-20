using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AnimalCrossingTracker.Data;
using AnimalCrossingTracker.Models;
using AnimalCrossingTracker.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// API Key
var apiKey = Environment.GetEnvironmentVariable("NOOKIPEDIA_APIKEY");
builder.Services.Configure<NookipediaOptions>(options =>
{
    options.ApiKey = apiKey;
});


// DB Connection
var host = Environment.GetEnvironmentVariable("DB_HOST");
var user = Environment.GetEnvironmentVariable("DB_USER");
var pass = Environment.GetEnvironmentVariable("DB_PASS");
var db   = Environment.GetEnvironmentVariable("DB_NAME");

var connectionString = $"server={host};port=3306;database={db};user={user};password={pass};SslMode=None;AllowPublicKeyRetrieval=True;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


// 🔹 Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// 🔹 Agregar MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // necesario para Identity

// 🔹 Registrar servicio de ACNH
builder.Services.AddHttpClient<NookipediaService>(client =>
{
    client.BaseAddress = new Uri("https://api.nookipedia.com/");
    client.Timeout = TimeSpan.FromSeconds(20); // 20s por request
});
// 🔹 Configuración de la aplicación

var keyPath = "/var/data/protection-keys"; // ruta persistente de Render
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
    .SetApplicationName("AnimalCrossingTracker");

var app = builder.Build();

// 🔹 Pipeline de la aplicación
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 🔹 Rutas MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapRazorPages(); // necesario para páginas de login/register

app.Run();
