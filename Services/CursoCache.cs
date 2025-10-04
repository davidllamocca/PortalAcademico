using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using PortalAcademico.Data;

public class CursoCache : ICursoCache
{
    private readonly ApplicationDbContext _db;
    private readonly IDistributedCache _cache;
    private const string Key = "cursos_activos";

    public CursoCache(ApplicationDbContext db, IDistributedCache cache)
    { _db = db; _cache = cache; }

    public async Task<List<Curso>> GetCursosActivosAsync()
    {
        var json = await _cache.GetStringAsync(Key);
        if (json is not null)
            return JsonSerializer.Deserialize<List<Curso>>(json) ?? new();

        var data = await _db.Cursos.AsNoTracking().Where(c => c.Activo).OrderBy(c => c.HorarioInicio).ToListAsync();

        await _cache.SetStringAsync(
            Key,
            JsonSerializer.Serialize(data),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
            });

        return data;
    }

    public Task InvalidateAsync() => _cache.RemoveAsync(Key);
}
