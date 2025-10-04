using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // <- para la ext. AddRazorRuntimeCompilation
using PortalAcademico.Data;


var builder = WebApplication.CreateBuilder(args);

// DB + Identity
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDefaultIdentity<IdentityUser>(o => o.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// MVC + Razor Pages
var mvc = builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// (Opcional solo en dev) Runtime compilation
#if DEBUG
mvc.AddRazorRuntimeCompilation();
#endif

// Cache distribuida (Redis o memoria)
var redisCnx = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrWhiteSpace(redisCnx))
    builder.Services.AddStackExchangeRedisCache(o => o.Configuration = redisCnx);
else
    builder.Services.AddDistributedMemoryCache();

// Sesión
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(30);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

// Servicios propios
builder.Services.AddScoped<ICursoCache, CursoCache>();

var app = builder.Build();

// Seed
using (var scope = app.Services.CreateScope())
    await Seed.RunAsync(scope.ServiceProvider);

// Pipeline
if (app.Environment.IsDevelopment())
    app.UseMigrationsEndPoint();
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalogo}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
