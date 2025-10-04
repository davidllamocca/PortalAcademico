public interface ICursoCache
{
    Task<List<Curso>> GetCursosActivosAsync();
    Task InvalidateAsync();
}
