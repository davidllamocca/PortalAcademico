namespace PortalAcademico.Services;

public interface ICursoCache
{
    Task<List<Curso>> GetCursosActivosAsync();
    Task InvalidateAsync();
}
