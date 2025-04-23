using Backend_Sistema_Central.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsbController(ApplicationDbContext db) : ControllerBase
{
    public record AsignarDto(string Serial, string CertThumbprint, int UsuarioId);

    [HttpPost("asignar")]
    public async Task<IActionResult> Asignar(AsignarDto dto)
    {
        if (db.DispositivosUSB.Any(u => u.Serial == dto.Serial))
            return Conflict("USB ya asignado");

        db.DispositivosUSB.Add(new DispositivoUSB
        {
            Serial = dto.Serial,
            CertThumbprint = dto.CertThumbprint,
            FechaAsignacion = DateTime.UtcNow,
            UsuarioId = dto.UsuarioId
        });
        await db.SaveChangesAsync();
        return Ok("USB asignado");
    }
}
