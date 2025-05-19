// Controllers/UsbController.cs
using System.Text.RegularExpressions;
using Backend_Sistema_Central.Models;
using Backend_Sistema_Central.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/usb")]
// ─────────── inyección por constructor primario (C# 12) ───────────
public class UsbController(ApplicationDbContext _db, IUsbStatusService _status) : ControllerBase
{
    /* ─────────────────── 1. Asignar vía JSON ─────────────────── */
    public record AsignarDto(string Serial, int UsuarioId);

    [HttpPost("asignar")]
    public async Task<IActionResult> Asignar([FromBody] AsignarDto dto)
    {
        var usb = await _db.DispositivosUSB
                           .FirstOrDefaultAsync(u => u.Serial == dto.Serial);

        if (usb is null)            return NotFound("Serial inexistente");
        if (usb.UsuarioId != null)  return Conflict("USB ya asignado");

        usb.UsuarioId = dto.UsuarioId;
        await _db.SaveChangesAsync();
        return Ok("USB asignado al usuario");
    }

    /* ─────────────── 2. Enlace rápido vía URL ─────────────── */
    // POST api/usb/0401.../link/2
    [HttpPost("{serial}/link/{userId:int}")]
    public async Task<IActionResult> Link(string serial, int userId)
    {
        var usb  = await _db.DispositivosUSB.FirstOrDefaultAsync(u => u.Serial == serial);
        var user = await _db.Usuarios        .FirstOrDefaultAsync(u => u.Id     == userId);

        if (usb  is null || user is null) return NotFound();
        if (usb.UsuarioId != null)        return Conflict("USB ya asignado");

        usb.UsuarioId = userId;
        await _db.SaveChangesAsync();
        return Ok(new { usb.Id, usb.Serial, usb.UsuarioId });
    }

    /* ─────────────── 3. Estado online del USB ─────────────── */
    // GET api/usb/5/online  → true / false
    [HttpGet("{serial}/online")]
    public ActionResult<bool> IsUsbOnline(string serial)
    => Ok(_status.IsUsbOnline(serial));



    /* ──────── (opcional) versión con validación completa ────────
    public record AsignarAvanzadoDto(string Serial, string Thumbprint, int UsuarioId);

    [HttpPost("asignar-adv")]
    public async Task<IActionResult> AsignarAvanzado([FromBody] AsignarAvanzadoDto dto)
    {
        if (!Regex.IsMatch(dto.Serial, @"^[0-9A-Fa-f]{8,32}$"))
            return BadRequest("Serial inválido (8-32 caracteres hex)");

        if (_db.DispositivosUSB.Any(u => u.Serial == dto.Serial))
            return Conflict("USB ya asignado");

        _db.DispositivosUSB.Add(new DispositivoUSB
        {
            Serial     = dto.Serial.ToUpperInvariant(),
            Thumbprint = dto.Thumbprint.ToUpperInvariant(),
            FechaAlta  = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok("USB registrado correctamente");
    }
    */
}
