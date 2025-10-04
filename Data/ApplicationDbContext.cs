using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PortalAcademico.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {}

        public DbSet<Curso> Cursos => Set<Curso>();
        public DbSet<Matricula> Matriculas => Set<Matricula>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // Curso.Codigo único
            b.Entity<Curso>()
             .HasIndex(c => c.Codigo)
             .IsUnique();

            // Check: Créditos > 0
            b.Entity<Curso>()
             .ToTable(t => t.HasCheckConstraint("CK_Curso_Creditos_Positive", "Creditos > 0"));

            // Check: HorarioInicio < HorarioFin
            b.Entity<Curso>()
             .ToTable(t => t.HasCheckConstraint("CK_Curso_Horario", "HorarioInicio < HorarioFin"));

            // Única matrícula por (CursoId, UsuarioId)
            b.Entity<Matricula>()
             .HasIndex(m => new { m.CursoId, m.UsuarioId })
             .IsUnique();
        }
    }
}
