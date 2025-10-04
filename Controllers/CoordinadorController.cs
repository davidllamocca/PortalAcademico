using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Services;     // <-- importa ICursoCache

namespace PortalAcademico.Controllers   // <-- opcional pero recomendable
{
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ICursoCache _cursoCache;

        public CoordinadorController(ApplicationDbContext db, ICursoCache cursoCache)
        {
            _db = db;
            _cursoCache = cursoCache;
        }

        public IActionResult Index() => RedirectToAction(nameof(Cursos));

        // --------- CURSOS ----------
        [HttpGet]
        public async Task<IActionResult> Cursos()
        {
            var cursos = await _db.Cursos
                .AsNoTracking()
                .OrderBy(c => c.HorarioInicio)
                .ToListAsync();

            var counts = await _db.Matriculas
                .Where(m => m.Estado != EstadoMatricula.Cancelada)
                .GroupBy(m => m.CursoId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            ViewBag.Inscritos = counts;
            return View(cursos);
        }

        [HttpGet]
        public IActionResult Crear() => View(new Curso());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Curso model)
        {
            if (model.HorarioFin <= model.HorarioInicio)
                ModelState.AddModelError(nameof(model.HorarioFin), "El HorarioFin debe ser mayor a HorarioInicio.");
            if (!ModelState.IsValid) return View(model);

            _db.Cursos.Add(model);
            await _db.SaveChangesAsync();
            await _cursoCache.InvalidateAsync();
            TempData["Ok"] = "Curso creado.";
            return RedirectToAction(nameof(Cursos));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var curso = await _db.Cursos.FindAsync(id);
            if (curso is null) return NotFound();
            return View(curso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Curso model)
        {
            if (id != model.Id) return BadRequest();
            if (model.HorarioFin <= model.HorarioInicio)
                ModelState.AddModelError(nameof(model.HorarioFin), "El HorarioFin debe ser mayor a HorarioInicio.");
            if (!ModelState.IsValid) return View(model);

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            await _cursoCache.InvalidateAsync();
            TempData["Ok"] = "Curso actualizado.";
            return RedirectToAction(nameof(Cursos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var curso = await _db.Cursos.FindAsync(id);
            if (curso is null) return NotFound();

            curso.Activo = !curso.Activo;
            await _db.SaveChangesAsync();
            await _cursoCache.InvalidateAsync();
            TempData["Ok"] = curso.Activo ? "Curso activado." : "Curso desactivado.";
            return RedirectToAction(nameof(Cursos));
        }

        // --------- MATRÍCULAS ----------
        [HttpGet]
        public async Task<IActionResult> Matriculas(int cursoId)
        {
            var curso = await _db.Cursos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cursoId);
            if (curso is null) return NotFound();

            var q = from m in _db.Matriculas.AsNoTracking().Where(x => x.CursoId == cursoId)
                    join u in _db.Users on m.UsuarioId equals u.Id into lj
                    from u in lj.DefaultIfEmpty()
                    orderby m.FechaRegistro descending
                    select new MatriculaRow
                    {
                        Id = m.Id,
                        UsuarioId = m.UsuarioId,
                        UsuarioEmail = u != null ? u.Email! : "(sin usuario)",
                        Estado = m.Estado,
                        FechaRegistro = m.FechaRegistro
                    };

            ViewBag.Curso = curso;
            var rows = await q.ToListAsync();
            return View(rows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(int id)
        {
            var m = await _db.Matriculas.FindAsync(id);
            if (m is null) return NotFound();

            m.Estado = EstadoMatricula.Confirmada;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Matrícula confirmada.";
            return RedirectToAction(nameof(Matriculas), new { cursoId = m.CursoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var m = await _db.Matriculas.FindAsync(id);
            if (m is null) return NotFound();

            m.Estado = EstadoMatricula.Cancelada;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Matrícula cancelada.";
            return RedirectToAction(nameof(Matriculas), new { cursoId = m.CursoId });
        }

        public class MatriculaRow
        {
            public int Id { get; set; }
            public string UsuarioId { get; set; } = "";
            public string UsuarioEmail { get; set; } = "";
            public EstadoMatricula Estado { get; set; }
            public DateTime FechaRegistro { get; set; }
        }
    }
}
