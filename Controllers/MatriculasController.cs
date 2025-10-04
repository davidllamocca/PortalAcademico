using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;

[Authorize]
public class MatriculasController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _users;

    public MatriculasController(ApplicationDbContext db, UserManager<IdentityUser> users)
    { _db = db; _users = users; }

    [HttpPost]
    public async Task<IActionResult> Inscribirse(int cursoId)
    {
        var curso = await _db.Cursos.FirstOrDefaultAsync(c => c.Id == cursoId && c.Activo);
        if (curso is null) return NotFound();

        var user = await _users.GetUserAsync(User);
        var uid = user!.Id;

        // 1) Ya inscrito en el mismo curso
        var ya = await _db.Matriculas.AnyAsync(m => m.CursoId == cursoId && m.UsuarioId == uid && m.Estado != EstadoMatricula.Cancelada);
        if (ya)
        {
            TempData["Error"] = "Ya estás matriculado en este curso.";
            return RedirectToAction("Detalle", "Catalogo", new { id = cursoId });
        }

        // 2) Cupo
        var usados = await _db.Matriculas.CountAsync(m => m.CursoId == cursoId && m.Estado != EstadoMatricula.Cancelada);
        if (usados >= curso.CupoMaximo)
        {
            TempData["Error"] = "No hay cupos disponibles.";
            return RedirectToAction("Detalle", "Catalogo", new { id = cursoId });
        }

        // 3) Solape de horario con otras matrículas del usuario
        var otros = await _db.Matriculas
            .Include(m => m.Curso)
            .Where(m => m.UsuarioId == uid && m.Estado != EstadoMatricula.Cancelada)
            .ToListAsync();

        bool solapa = otros.Any(m =>
            m.Curso!.HorarioInicio < curso.HorarioFin &&
            curso.HorarioInicio < m.Curso.HorarioFin);

        if (solapa)
        {
            TempData["Error"] = "Tienes choque de horario con otra matrícula.";
            return RedirectToAction("Detalle", "Catalogo", new { id = cursoId });
        }

        _db.Matriculas.Add(new Matricula
        {
            CursoId = cursoId,
            UsuarioId = uid,
            Estado = EstadoMatricula.Pendiente
        });
        await _db.SaveChangesAsync();

        TempData["Ok"] = "Matrícula registrada en estado Pendiente.";
        return RedirectToAction("Detalle", "Catalogo", new { id = cursoId });
    }
}
