// Controllers/UsbController.cs
using System.Text.RegularExpressions;
using Backend_Sistema_Central.Models;
using Backend_Sistema_Central.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/usb")]
public class UsbController(ApplicationDbContext _db, IUsbStatusService _status) : ControllerBase
{
    // Ahora el DTO usa UsuarioRut en vez de UsuarioId
    public record AsignarDto(string Serial, string UsuarioRut);

    /// POST /api/usb/asignar
    [HttpPost("asignar")]
    public async Task<IActionResult> Asignar([FromBody] AsignarDto dto)
    {
        var usb = await _db.DispositivosUSB.FirstOrDefaultAsync(u => u.Serial == dto.Serial);
        if (usb is null) return NotFound("Serial inexistente");
        if (usb.UsuarioId != null) return Conflict("USB ya asignado");

        var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Rut == dto.UsuarioRut);
        if (user is null) return NotFound("Usuario no encontrado");

        usb.UsuarioId = user.Id;
        await _db.SaveChangesAsync();
        return Ok(new { usb.Serial, user.Rut, Message = "USB asignado al usuario" });
    }

    // Puedes dejar los otros métodos para compatibilidad o administración.
    // POST /api/usb/{serial}/link/{rut}
    [HttpPost("{serial}/link/{rut}")]
    public async Task<IActionResult> Link(string serial, string rut)
    {
        var usb = await _db.DispositivosUSB.FirstOrDefaultAsync(u => u.Serial == serial);
        var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Rut == rut);

        if (usb is null || user is null) return NotFound();
        if (usb.UsuarioId != null) return Conflict("USB ya asignado");

        usb.UsuarioId = user.Id;
        await _db.SaveChangesAsync();
        return Ok(new { usb.Serial, user.Rut, Message = "Enlace completado" });
    }

    [HttpGet("{serial}/online")]
    public ActionResult<bool> IsUsbOnline(string serial)
        => Ok(_status.IsUsbOnline(serial));
}




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
