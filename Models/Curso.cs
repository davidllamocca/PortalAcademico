// Models/Curso.cs
using System.ComponentModel.DataAnnotations;

public class Curso
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string Codigo { get; set; } = "";

    [Required, StringLength(120)]
    public string Nombre { get; set; } = "";

    [Range(1, 30)]
    public int Creditos { get; set; }   // Creditos > 0

    [Range(1, 500)]
    public int CupoMaximo { get; set; }

    [DataType(DataType.Time)]
    public TimeOnly HorarioInicio { get; set; }

    [DataType(DataType.Time)]
    public TimeOnly HorarioFin { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}

