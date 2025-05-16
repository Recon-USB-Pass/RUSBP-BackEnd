using Backend_Sistema_Central.DTOs;
using Backend_Sistema_Central.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/logs")]
public class LogsController(ApplicationDbContext db, ILogger<LogsController> logger) : ControllerBase
{
    // GET /api/logs?page=1&pageSize=50
    [HttpGet]
    public async Task<IEnumerable<LogActividad>> Get(int page = 1, int pageSize = 50) =>
        await db.Logs.OrderByDescending(l => l.Timestamp)
                     .Skip((page - 1) * pageSize)
                     .Take(pageSize)
                     .ToListAsync();

    // POST /api/logs  (lote enviado por agente)
    [HttpPost]
    public async Task<IActionResult> PostBatch([FromBody] List<LogEventDto> eventos)
    {
        if (eventos == null || eventos.Count == 0)
            return BadRequest("El lote de logs está vacío.");

        var nuevos = new List<LogActividad>();
        var procesados = new List<string>();

        foreach (var e in eventos)
        {
            if (await db.Logs.AnyAsync(l => l.EventId == e.EventId))
                continue; // Duplicado: se ignora

            nuevos.Add(new LogActividad
            {
                EventId    = e.EventId,
                UserRut    = e.UserRut,
                UsbSerial  = e.UsbSerial.ToUpperInvariant(),
                EventType  = e.EventType,
                Ip         = e.Ip,
                Mac        = e.Mac,
                Timestamp  = e.Timestamp
            });
            procesados.Add(e.EventId);
        }

        if (nuevos.Count > 0)
        {
            db.Logs.AddRange(nuevos);
            await db.SaveChangesAsync();
        }

        logger.LogInformation("Batch logs recibidos: {Count}", nuevos.Count);

        return Ok(procesados);
    }
}
