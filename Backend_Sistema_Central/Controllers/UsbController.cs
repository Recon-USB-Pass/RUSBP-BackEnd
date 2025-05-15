// Controllers/UsbController.cs
using Backend_Sistema_Central.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/usb")]
public class UsbController(ApplicationDbContext db) : ControllerBase
{
    /* ───────────── 1. Asignación vía JSON ───────────── */
    public record AsignarDto(string Serial, int UsuarioId);

    [HttpPost("asignar")]
    public async Task<IActionResult> Asignar([FromBody] AsignarDto dto)
    {
        var usb = await db.DispositivosUSB
                          .FirstOrDefaultAsync(u => u.Serial == dto.Serial);

        if (usb is null)           return NotFound("Serial inexistente");
        if (usb.UsuarioId != null) return Conflict("USB ya asignado");

        usb.UsuarioId = dto.UsuarioId;
        await db.SaveChangesAsync();
        return Ok("USB asignado al usuario");
    }

    /* ───────────── 2. Enlace rápido vía URL ──────────── */
    // POST api/usb/0401.../link/2
    [HttpPost("{serial}/link/{userId:int}")]
    public async Task<IActionResult> Link(string serial, int userId)
    {
        var usb  = await db.DispositivosUSB.FirstOrDefaultAsync(u => u.Serial == serial);
        var user = await db.Usuarios        .FirstOrDefaultAsync(u => u.Id     == userId);

        if (usb  is null || user is null) return NotFound();
        if (usb.UsuarioId != null)        return Conflict("USB ya asignado");

        usb.UsuarioId = userId;
        await db.SaveChangesAsync();
        return Ok(new { usb.Id, usb.Serial, usb.UsuarioId });
    }
}


    /*
    public record AsignarDto(string Serial, string Thumbprint, int UsuarioId);

    [HttpPost("asignar")]
    public async Task<IActionResult> Asignar(AsignarDto dto)
    {
        // Validar que el serial no sea vacío y tenga formato alfanumérico simple
        if (string.IsNullOrWhiteSpace(dto.Serial) ||
            !Regex.IsMatch(dto.Serial, @"^[0-9A-Fa-f]{8,32}$"))
        {
            return BadRequest("Serial inválido (debe contener 8-32 caracteres hexadecimales)");
        }

        if (db.DispositivosUSB.Any(u => u.Serial == dto.Serial))
            return Conflict("USB ya asignado");

        db.DispositivosUSB.Add(new DispositivoUSB
        {
            Serial      = dto.Serial.ToUpperInvariant(),
            Thumbprint  = dto.Thumbprint.ToUpperInvariant(),
            FechaAlta   = DateTime.UtcNow,
        });

        await db.SaveChangesAsync();
        return Ok("USB asignado correctamente");
    }

    */
