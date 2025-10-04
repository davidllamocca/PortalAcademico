using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class Seed
{
    public static async Task RunAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<PortalAcademico.Data.ApplicationDbContext>();
        await ctx.Database.MigrateAsync();

        // Cursos
        if (!await ctx.Cursos.AnyAsync())
        {
            ctx.Cursos.AddRange(
                new Curso{ Codigo="CS101", Nombre="Algoritmos", Creditos=4, CupoMaximo=30, HorarioInicio=new(8,0), HorarioFin=new(10,0), Activo=true },
                new Curso{ Codigo="DB201", Nombre="BD I",      Creditos=3, CupoMaximo=25, HorarioInicio=new(10,0), HorarioFin=new(12,0), Activo=true },
                new Curso{ Codigo="SE301", Nombre="Web MVC",   Creditos=3, CupoMaximo=20, HorarioInicio=new(14,0), HorarioFin=new(16,0), Activo=true }
            );
            await ctx.SaveChangesAsync();
        }

        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        const string role = "Coordinador";
        if (!await roleMgr.RoleExistsAsync(role)) await roleMgr.CreateAsync(new IdentityRole(role));

        var email = "coordinador@usmp.pe";
        var user = await userMgr.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            await userMgr.CreateAsync(user, "Coord123$");
            await userMgr.AddToRoleAsync(user, role);
        }
    }
}
