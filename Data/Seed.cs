using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PortalAcademico.Data;

public static class Seed
{
    /// <summary>
    /// Aplica migraciones y siembra datos mínimos:
    /// - 3 cursos de ejemplo
    /// - Rol "Coordinador"
    /// - Usuario coordinador@usmp.pe (pass: Coord123$) en el rol
    /// </summary>
    public static async Task RunAsync(IServiceProvider sp)
    {
        var ctx      = sp.GetRequiredService<ApplicationDbContext>();
        var userMgr  = sp.GetRequiredService<UserManager<IdentityUser>>();
        var roleMgr  = sp.GetRequiredService<RoleManager<IdentityRole>>();

        // 1) Migraciones
        await ctx.Database.MigrateAsync();

        // 2) Cursos iniciales (solo si no hay)
        if (!await ctx.Cursos.AnyAsync())
        {
            ctx.Cursos.AddRange(
                new Curso { Codigo="CS101", Nombre="Algoritmos", Creditos=4, CupoMaximo=30, HorarioInicio=new(8,0),  HorarioFin=new(10,0), Activo=true },
                new Curso { Codigo="DB201", Nombre="BD I",      Creditos=3, CupoMaximo=25, HorarioInicio=new(10,0), HorarioFin=new(12,0), Activo=true },
                new Curso { Codigo="SE301", Nombre="Web MVC",   Creditos=3, CupoMaximo=20, HorarioInicio=new(14,0), HorarioFin=new(16,0), Activo=true }
            );
            await ctx.SaveChangesAsync();
        }

        // 3) Rol Coordinador
        const string rolCoord = "Coordinador";
        if (!await roleMgr.RoleExistsAsync(rolCoord))
            await roleMgr.CreateAsync(new IdentityRole(rolCoord));

        // 4) Usuario Coordinador (demo)
        const string email = "coordinador@usmp.pe";
        const string pass  = "Coord123$"; // cámbialo si quieres
        var user = await userMgr.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            var create = await userMgr.CreateAsync(user, pass);
            if (!create.Succeeded)
                throw new Exception("No se pudo crear el usuario coordinador: " +
                                    string.Join("; ", create.Errors.Select(e => $"{e.Code}:{e.Description}")));
        }
        if (!await userMgr.IsInRoleAsync(user, rolCoord))
            await userMgr.AddToRoleAsync(user, rolCoord);
    }
}
