using Backend_Sistema_Central.DTOs;
using Backend_Sistema_Central.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/logs")]
public class LogsController(ApplicationDbContext db) : ControllerBase
{
    // GET /api/logs?page=1&pageSize=50
    [HttpGet]
    public async Task<IEnumerable<LogActividad>> Get(int page = 1, int pageSize = 50) =>
        await db.Logs.OrderByDescending(l => l.FechaHora)
                     .Skip((page - 1) * pageSize)
                     .Take(pageSize)
                     .ToListAsync();

    // POST /api/logs  (lote enviado por agente)
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] IEnumerable<LogEntryDto> batch)
    {
        foreach (var e in batch)
        {
            var usb = await db.DispositivosUSB.FirstOrDefaultAsync(u => u.Serial == e.Serial);
            if (usb is null) continue; // descartar si USB desconocido

            db.Logs.Add(new LogActividad
            {
                UsuarioId  = usb.UsuarioId,
                TipoEvento = e.TipoEvento,
                IP         = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                MAC        = "",                // opcional en lote
                FechaHora  = e.FechaHora.ToUniversalTime(),
                Detalle    = e.Detalle
            });
        }
        await db.SaveChangesAsync();
        return Accepted();
    }
}
