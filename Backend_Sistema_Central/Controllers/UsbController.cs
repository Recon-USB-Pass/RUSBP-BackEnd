// Backend_Sistema_Central/Controllers/UsbController.cs
using Backend_Sistema_Central.Models;
using Backend_Sistema_Central.Services;
using Backend_Sistema_Central.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/usb")]
public class UsbController(ApplicationDbContext db, IUsbStatusService status) : ControllerBase
{
    // 0. Alta parcial (serial + thumbprint opcional)
    public record CrearUsbDto(string Serial, string? Thumbprint = null);

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearUsbDto dto)
    {
        var serial = dto.Serial.Trim().ToUpperInvariant();
        if (await db.DispositivosUSB.AnyAsync(u => u.Serial == serial))
            return Conflict($"Serial '{serial}' ya registrado.");

        db.DispositivosUSB.Add(new DispositivoUSB
        {
            Serial     = serial,
            Thumbprint = dto.Thumbprint,
            FechaAlta  = DateTime.UtcNow,
            Revoked    = false
        });
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Crear), new { serial });
    }

    // 0-bis. Alta definitiva con cipher/tag y rol
    public record RegisterDto(string Serial, string Cipher, string Tag, UsbRole Rol);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var serial = dto.Serial.Trim().ToUpperInvariant();
        if (await db.DispositivosUSB.AnyAsync(u => u.Serial == serial))
            return Conflict("Serial ya registrado");

        db.DispositivosUSB.Add(new DispositivoUSB
        {
            Serial    = serial,
            RpCipher  = Convert.FromBase64String(dto.Cipher),
            RpTag     = Convert.FromBase64String(dto.Tag),
            Rol       = dto.Rol,
            FechaAlta = DateTime.UtcNow,
            Revoked   = false
        });
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Register), new { serial });
    }

    // 1. Asignar USB a usuario existente
    public record AsignarDto(string Serial, string UsuarioRut);

    [HttpPost("asignar")]
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

    // 2. Link rápido vía URL (opcional)
    [HttpPost("{serial}/link/{rut}")]
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

    // 3. Online-ping (SignalR)
    [HttpGet("{serial}/online")]
    public ActionResult<bool> IsUsbOnline(string serial)
        => Ok(status.IsUsbOnline(serial.Trim().ToUpperInvariant()));

    // 4. RECOVER: Recupera el recovery pass cifrado según política de roles
    [HttpPost("recover")]
    public async Task<ActionResult<UsbRecoverResponseDto>> Recover([FromBody] UsbRecoverRequestDto dto)
    {
        var serial = dto.Serial.Trim().ToUpperInvariant();
        var usb = await db.DispositivosUSB
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Serial == serial && !u.Revoked);

        if (usb is null)
            return NotFound("USB no registrado o revocado.");

        // • 0 = Bootstrap (solo Root)
        if (dto.AgentType == UsbRole.Root && usb.Rol != UsbRole.Root)
            return Forbid("Solo Root puede recuperar claves de Root.");

        // • 1 = Admin (Root o Admin)
        if (dto.AgentType == UsbRole.Admin && usb.Rol == UsbRole.Employee)
            return Forbid("Admin no puede recuperar claves de Employee.");

        // • 2 = Employee (Root, Admin o Employee) → siempre OK

        var resp = new UsbRecoverResponseDto
        {
            Cipher = Convert.ToBase64String(usb.RpCipher),
            Tag    = Convert.ToBase64String(usb.RpTag),
            Rol    = usb.Rol
        };
        return Ok(resp);
    }
}
