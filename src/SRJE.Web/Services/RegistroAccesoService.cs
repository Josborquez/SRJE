using Serilog;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models.Entities;

namespace SRJE.Web.Services;

public class RegistroAccesoService : IRegistroAccesoService
{
    private readonly SrjeDbContext _db;

    public RegistroAccesoService(SrjeDbContext db)
    {
        _db = db;
    }

    public async Task RegistrarAsync(string usuario, string evento, string? ip)
    {
        try
        {
            var normalizado = usuario.Trim().ToLower();
            if (normalizado.Length > 50)
                normalizado = normalizado[..50];

            _db.LogAccesos.Add(new LogAcceso
            {
                Usuario = normalizado,
                Evento = evento,
                Ip = ip,
                Fecha = DateTime.Now
            });
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // El login/logout nunca debe fallar por un error de logging
            Log.Warning(ex, "No se pudo registrar el evento de acceso {Evento} para {Usuario}", evento, usuario);
        }
    }
}
