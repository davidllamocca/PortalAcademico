using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;

public class CatalogoController : Controller
{
    private readonly ApplicationDbContext _db;
    public CatalogoController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? nombre, int? minCred, int? maxCred, TimeOnly? hIni, TimeOnly? hFin)
    {
        if ((minCred ?? 0) < 0 || (maxCred ?? 0) < 0)
            ModelState.AddModelError("", "Los créditos no pueden ser negativos.");

        if (hIni.HasValue && hFin.HasValue && hFin <= hIni)
            ModelState.AddModelError("", "El HorarioFin debe ser mayor a HorarioInicio.");

        var q = _db.Cursos.AsNoTracking().Where(c => c.Activo);

        if (!string.IsNullOrWhiteSpace(nombre)) q = q.Where(c => c.Nombre.Contains(nombre));
        if (minCred.HasValue) q = q.Where(c => c.Creditos >= minCred);
        if (maxCred.HasValue) q = q.Where(c => c.Creditos <= maxCred);
        if (hIni.HasValue)     q = q.Where(c => c.HorarioInicio >= hIni);
        if (hFin.HasValue)     q = q.Where(c => c.HorarioFin   <= hFin);

        var cursos = await q.OrderBy(c => c.HorarioInicio).ToListAsync();
        return View(cursos);
    }

    // using Microsoft.EntityFrameworkCore;
    public async Task<IActionResult> Detalle(int id)
    {
        var curso = await _db.Cursos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
        if (curso is null) return NotFound();

        var usados = await _db.Matriculas
            .CountAsync(m => m.CursoId == id && m.Estado != EstadoMatricula.Cancelada);

        ViewBag.Usados = usados; // para la vista
        return View(curso);
    }

}
