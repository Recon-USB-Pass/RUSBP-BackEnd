// Controllers/UsbController.cs
using Backend_Sistema_Central.Models;
using Backend_Sistema_Central.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/usb")]
public class UsbController(ApplicationDbContext db, IUsbStatusService status) : ControllerBase
{
    /* 0. Registrar pendrive ------------------------------------------------ */
    public record CrearUsbDto(string Serial, string? Thumbprint = null);

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearUsbDto dto)
    {
        var serial = dto.Serial.Trim().ToUpperInvariant();
        var existente = await db.DispositivosUSB.FirstOrDefaultAsync(u => u.Serial == serial);
        if (existente is not null)
            return Conflict($"Serial '{serial}' ya registrado (id={existente.Id})");

        var usb = new DispositivoUSB
        {
            Serial     = serial,
            Thumbprint = dto.Thumbprint,
            FechaAlta  = DateTime.UtcNow,
            Revoked    = false
        };

        db.DispositivosUSB.Add(usb);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(Crear), new { id = usb.Id }, new { usb.Id, usb.Serial });
    }

    /* ═══════════════════════════════════════════════════════════════
       1)  Asignar USB a un usuario existente
       ═════════════════════════════════════════════════════════════ */
    public record AsignarDto(string Serial, string UsuarioRut);

    [HttpPost("asignar")]                     // POST  /api/usb/asignar
    public async Task<IActionResult> Asignar([FromBody] AsignarDto dto)
    {
        var serial = dto.Serial.Trim().ToUpperInvariant();

        var usb  = await db.DispositivosUSB.FirstOrDefaultAsync(u => u.Serial == serial);
        var user = await db.Usuarios       .FirstOrDefaultAsync(u => u.Rut    == dto.UsuarioRut);

        if (usb  is null) return NotFound("Serial inexistente");
        if (user is null) return NotFound("Usuario inexistente");

        if (usb.UsuarioId is not null && usb.UsuarioId != user.Id)
            return Conflict("USB ya asignado a otro usuario");

        usb.UsuarioId = user.Id;
        await db.SaveChangesAsync();
        return Ok(new { usb.Serial, user.Rut });
    }

    /* ═══════════════════════════════════════════════════════════════
       2)  Enlace rápido vía URL  (opcional)
       ═════════════════════════════════════════════════════════════ */
    [HttpPost("{serial}/link/{rut}")]         // POST  /api/usb/{serial}/link/{rut}
    public async Task<IActionResult> Link(string serial, string rut)
    {
        serial = serial.Trim().ToUpperInvariant();

        var usb  = await db.DispositivosUSB.FirstOrDefaultAsync(u => u.Serial == serial);
        var user = await db.Usuarios       .FirstOrDefaultAsync(u => u.Rut    == rut);

        if (usb is null || user is null) return NotFound();
        if (usb.UsuarioId != null && usb.UsuarioId != user.Id)
            return Conflict("USB ya asignado a otro usuario");

        usb.UsuarioId = user.Id;
        await db.SaveChangesAsync();
        return Ok(new { usb.Serial, user.Rut });
    }

    /* ═══════════════════════════════════════════════════════════════
       3)  ¿Está online?
       ═════════════════════════════════════════════════════════════ */
    [HttpGet("{serial}/online")]              // GET  /api/usb/{serial}/online
    public ActionResult<bool> IsUsbOnline(string serial)
        => Ok(status.IsUsbOnline(serial.Trim().ToUpperInvariant()));
}
