using Backend_Sistema_Central.DTOs;
using Backend_Sistema_Central.Models;
using Backend_Sistema_Central.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    ApplicationDbContext        db,
    ICertificateValidator       certValidator,
    IChallengeService           challenges) : ControllerBase
{
    // ───────────────────────────────────────── Verify USB
    [HttpPost("verify-usb")]
    public async Task<IActionResult> VerifyUsb([FromBody] UsbVerificationDto dto)
    {
        if (!certValidator.IsSignedByRoot(dto.CertPem, out var cert))
            return Unauthorized("Certificado no firmado por la CA");

        var thumb = Convert.ToHexString(cert.GetCertHash());

        var existing = await db.DispositivosUSB
                               .FirstOrDefaultAsync(u => u.Serial == dto.Serial);

        if (existing is null)
        {
            db.DispositivosUSB.Add(new DispositivoUSB
            {
                Serial     = dto.Serial,
                Thumbprint = thumb,
                FechaAlta  = DateTime.UtcNow
            });
        }
        else
        {
            existing.Thumbprint = thumb;
            existing.Revoked    = false;
        }

        await db.SaveChangesAsync();

        // Entregamos un challenge que debe firmar el USB
        var challenge = challenges.Create(dto.Serial);
        return Ok(Convert.ToBase64String(challenge));
    }

    // ───────────────────────────────────────── Login con firma
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var challenge = challenges.Get(dto.Serial);
        if (challenge.Length == 0) return Unauthorized("Challenge vencido");

        var sigBytes = Convert.FromBase64String(dto.SignatureBase64);
        if (!certValidator.VerifySignature(dto.Serial, challenge, sigBytes))
            return Unauthorized("Firma inválida");

        // Pin
        var usb = await db.DispositivosUSB.Include(u => u.Usuario)
                                          .FirstOrDefaultAsync(u => u.Serial == dto.Serial);
        if (usb?.Usuario is null) return Unauthorized("USB sin usuario");

        if (!BCrypt.Net.BCrypt.Verify(dto.Pin, usb.Usuario.PinHash))
            return Unauthorized("PIN incorrecto");

        // Log OK
        db.Logs.Add(new LogActividad
        {
            UsuarioId  = usb.UsuarioId,
            TipoEvento = "LoginOK",
            IP         = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
            MAC        = dto.MacAddress,
            FechaHora  = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        return Ok("Login exitoso");
    }
}



/*
using Backend_Sistema_Central.DTOs;
using Backend_Sistema_Central.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>
    ///  Login de prueba: compara UsuarioId + PIN.
    ///  No valida certificado ni USB mientras estemos en HTTP.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == dto.UsuarioId);
        if (usuario is null) return Unauthorized("Usuario no encontrado");

        if (!BCrypt.Net.BCrypt.Verify(dto.Pin, usuario.PinHash))
            return Unauthorized("PIN incorrecto");

        db.Logs.Add(new LogActividad
        {
            UsuarioId  = usuario.Id,
            TipoEvento = "LoginOK",
            IP         = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
            MAC        = dto.MacAddress,
            FechaHora  = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        return Ok("Login exitoso");
    }
}
*/

/*

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
        //var cert = HttpContext.Connection.ClientCertificate;
        //if (cert is null) return Unauthorized("Certificado requerido");

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

*/