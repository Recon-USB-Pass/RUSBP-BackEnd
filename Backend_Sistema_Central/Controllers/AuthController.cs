using Backend_Sistema_Central.DTOs;
using Backend_Sistema_Central.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ApplicationDbContext db) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var cert = HttpContext.Connection.ClientCertificate;
        if (cert is null) return Unauthorized("Certificado requerido");

        var usb = await db.DispositivosUSB.Include(u => u.Usuario)
                      .FirstOrDefaultAsync(u => u.CertThumbprint == cert.Thumbprint);
        if (usb is null) return Unauthorized("USB no registrado");

        if (!BCrypt.Net.BCrypt.Verify(dto.Pin, usb.Usuario.PinHash))
            return Unauthorized("PIN incorrecto");

        db.Logs.Add(new LogActividad
        {
            UsuarioId = usb.UsuarioId,
            TipoEvento = "LoginOK",
            IP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
            MAC = dto.MacAddress,
            FechaHora = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        return Ok("Login exitoso");
    }
}
