// Controllers/UsbController.cs
using System.Text.RegularExpressions;
using Backend_Sistema_Central.Models;
using Backend_Sistema_Central.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_Sistema_Central.Controllers;

// Controllers/UsbController.cs
[ApiController]
[Route("api/usb")]
public class UsbController(ApplicationDbContext db, IUsbStatusService status) : ControllerBase
{
    /* 1.  Asignar vía JSON ------------- */
    public record AsignarDto(string Serial, string UsuarioRut);

    [HttpPost("asignar")]
    public async Task<IActionResult> Asignar([FromBody] AsignarDto dto)
    {
        var usb  = await db.DispositivosUSB.FirstOrDefaultAsync(u => u.Serial == dto.Serial);
        var user = await db.Usuarios       .FirstOrDefaultAsync(u => u.Rut    == dto.UsuarioRut);

        if (usb  is null) return NotFound("Serial inexistente");
        if (user is null) return NotFound("Usuario inexistente");

        if (usb.UsuarioId is not null && usb.UsuarioId != user.Id)
            return Conflict("USB ya asignado a otro usuario");

        usb.UsuarioId = user.Id;
        await db.SaveChangesAsync();
        return Ok(new { usb.Serial, user.Rut });
    }

    /* 2.  Enlace rápido vía URL -------- */            // opcional
    [HttpPost("{serial}/link/{rut}")]
    public async Task<IActionResult> Link(string serial, string rut)
    {
        var usb  = await db.DispositivosUSB.FirstOrDefaultAsync(u => u.Serial == serial);
        var user = await db.Usuarios       .FirstOrDefaultAsync(u => u.Rut    == rut);

        if (usb is null || user is null) return NotFound();
        if (usb.UsuarioId != null && usb.UsuarioId != user.Id)
            return Conflict("USB ya asignado a otro usuario");

        usb.UsuarioId = user.Id;
        await db.SaveChangesAsync();
        return Ok(new { usb.Serial, user.Rut });
    }

    /* 3.  ¿Está online? ---------------- */
    [HttpGet("{serial}/online")]
    public ActionResult<bool> IsUsbOnline(string serial) => Ok(status.IsUsbOnline(serial));
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
