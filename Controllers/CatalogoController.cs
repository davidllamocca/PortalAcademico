using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;          // para Session.SetInt32/SetString
using PortalAcademico.Data;
using PortalAcademico.Services;

          // ICursoCache

public class CatalogoController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICursoCache _cursoCache;

    public CatalogoController(ApplicationDbContext db, ICursoCache cursoCache)
    {
        _db = db;
        _cursoCache = cursoCache;
    }

    // Lista desde CACHE (Redis o memoria, según Program.cs)
    public async Task<IActionResult> Index(string? nombre, int? minCred, int? maxCred, TimeOnly? hIni, TimeOnly? hFin)
    {
        if ((minCred ?? 0) < 0 || (maxCred ?? 0) < 0)
            ModelState.AddModelError("", "Los créditos no pueden ser negativos.");
        if (hIni.HasValue && hFin.HasValue && hFin <= hIni)
            ModelState.AddModelError("", "El HorarioFin debe ser mayor a HorarioInicio.");

        // 1) Trae del cache (60s) la lista de cursos activos ordenados
        var cursos = await _cursoCache.GetCursosActivosAsync();

        // 2) Aplica filtros en memoria (simple y suficiente para el examen)
        if (!string.IsNullOrWhiteSpace(nombre))
            cursos = cursos.Where(c => c.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase)).ToList();
        if (minCred.HasValue)
            cursos = cursos.Where(c => c.Creditos >= minCred).ToList();
        if (maxCred.HasValue)
            cursos = cursos.Where(c => c.Creditos <= maxCred).ToList();
        if (hIni.HasValue)
            cursos = cursos.Where(c => c.HorarioInicio >= hIni).ToList();
        if (hFin.HasValue)
            cursos = cursos.Where(c => c.HorarioFin <= hFin).ToList();

        return View(cursos.OrderBy(c => c.HorarioInicio).ToList());
    }

    // Guarda "último curso" en sesión y pasa contadores a la vista
    public async Task<IActionResult> Detalle(int id)
    {
        var curso = await _db.Cursos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
        if (curso is null) return NotFound();

        // Sesión para botón "Volver a ..."
        HttpContext.Session.SetInt32("LastCursoId", curso.Id);
        HttpContext.Session.SetString("LastCursoNombre", curso.Nombre);

        var usados = await _db.Matriculas
            .CountAsync(m => m.CursoId == id && m.Estado != EstadoMatricula.Cancelada);
        ViewBag.Usados = usados;

        return View(curso);
    }
}
