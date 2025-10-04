
using System.ComponentModel.DataAnnotations;

public enum EstadoMatricula { Pendiente, Confirmada, Cancelada }

public class Matricula
{
    public int Id { get; set; }

    [Required] public int CursoId { get; set; }
    [Required] public string UsuarioId { get; set; } = "";
    [Required] public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    [Required] public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;

    public Curso? Curso { get; set; }
}