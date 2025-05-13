using System.Text.RegularExpressions;
using Backend_Sistema_Central.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsbController(ApplicationDbContext db) : ControllerBase
{
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
            UsuarioId   = dto.UsuarioId
        });

        await db.SaveChangesAsync();
        return Ok("USB asignado correctamente");
    }
}
