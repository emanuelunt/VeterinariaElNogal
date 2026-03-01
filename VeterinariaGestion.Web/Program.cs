using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Infrastructure.Data;
using VeterinariaGestion.Infrastructure.Repositories;
using VeterinariaGestion.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ── SQLite ─────────────────────────────────────────────────────────────
builder.Services.AddDbContext<VeterinariaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("VeterinariaDB")));

// ── Autenticación Cookie ───────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath         = "/Account/Login";
        options.AccessDeniedPath  = "/Account/AccesoDenegado";
        options.ExpireTimeSpan    = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// ── Repositorios ───────────────────────────────────────────────────────
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IClienteRepository,  ClienteRepository>();
builder.Services.AddScoped<IVentaRepository,    VentaRepository>();
builder.Services.AddScoped<IMascotaRepository,  MascotaRepository>();

// ── Servicios ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IClienteService,  ClienteService>();
builder.Services.AddScoped<IVentaService,    VentaService>();
builder.Services.AddScoped<IAuthService,     AuthService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ── Migraciones automáticas ────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VeterinariaDbContext>();
    db.Database.Migrate();
}

// Crear usuario admin por defecto si no existe
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VeterinariaDbContext>();
    context.Database.Migrate();

    if (!context.Usuarios.Any())
    {
        context.Usuarios.Add(new VeterinariaGestion.Core.Entities.Usuario
        {
            NombreUsuario = "admin",
            Email         = "admin@veterinaria.com",
            PasswordHash  = BCrypt.Net.BCrypt.HashPassword("Admin1234!"),
            FechaCreacion = DateTime.Now,
            Activo        = 1
        });
        context.SaveChanges();
        Console.WriteLine("✅ Usuario admin creado: admin / Admin1234!");
    }
}
app.Run();