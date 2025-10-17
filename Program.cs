using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AnimalCrossingTracker.Data;
using AnimalCrossingTracker.Models;
using AnimalCrossingTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Conexión a la base de datos MySQL (cPanel)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
           .EnableSensitiveDataLogging()        // opcional: útil para debug
);

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
