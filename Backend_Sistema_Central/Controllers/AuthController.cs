// Backend_Sistema_Central/Controllers/AuthController.cs
using System.Security.Cryptography.X509Certificates;
using Backend_Sistema_Central.DTOs;
using Backend_Sistema_Central.Models;
using Backend_Sistema_Central.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend_Sistema_Central.Controllers;

internal static partial class AuthHelpers
{
    internal static void DumpPin(string pin)
    {
        var dir  = Path.Combine(AppContext.BaseDirectory, "pin_dumps");
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, "last_pin.txt");
        File.AppendAllText(file, $"{DateTime.UtcNow:u}  PIN={pin}{Environment.NewLine}");
    }
}

[ApiController]
[Route("api/auth")]
public class AuthController(
        ApplicationDbContext  db,
        ICertificateValidator certValidator,
        IChallengeService     challenges,
        ILogger<AuthController> logger) : ControllerBase
{
    /* 1. Verificación inicial del USB (challenge) ───────────────── */
    [HttpPost("verify-usb")]
    public async Task<IActionResult> VerifyUsb([FromBody] UsbVerificationDto dto)
    {
        if (!certValidator.IsSignedByRoot(dto.CertPem, out X509Certificate2 cert))
            return Unauthorized("Certificado no firmado por la CA");

        string thumb  = Convert.ToHexString(cert.GetCertHash());
        string serial = dto.Serial.Trim().ToUpperInvariant();
        var usb = await db.DispositivosUSB.FirstOrDefaultAsync(u => u.Serial == serial);

        if (usb is null)
        {
            usb = new DispositivoUSB { Serial = serial, Thumbprint = thumb,
                                       FechaAlta = DateTime.UtcNow };
            db.DispositivosUSB.Add(usb);
        }
        else
        {
            usb.Thumbprint = thumb;
            usb.Revoked    = false;
        }
        await db.SaveChangesAsync();

        byte[] challenge = challenges.Create(serial, cert);
        return Ok(Convert.ToBase64String(challenge));
    }

    /* 2. Recuperar RP cifrada + jerarquía de roles ──────────────── */
    public record RecoverDto(string Serial, string SignatureBase64,
                             string Pin, AgentKind AgentType);

    [HttpPost("recover")]
    public async Task<IActionResult> Recover([FromBody] RecoverDto dto)
    {
        // 2.a — challenge vivo + firma válida
        if (!challenges.TryGet(dto.Serial, out X509Certificate2? cert, out string? challB64) ||
            !certValidator.VerifySignature(cert!, challB64!, dto.SignatureBase64))
            return Unauthorized("Challenge o firma inválida");

        // 2.b — USB y usuario asociados
        var usb = await db.DispositivosUSB.Include(u => u.Usuario)
                                          .FirstOrDefaultAsync(u => u.Serial == dto.Serial);
        if (usb?.Usuario == null)
            return Unauthorized("USB sin usuario asignado");

        // 2.c — PIN
        logger.LogDebug("PIN introducido => {Pin}", dto.Pin);
        AuthHelpers.DumpPin(dto.Pin);
        if (!BCrypt.Net.BCrypt.Verify(dto.Pin.Trim(), usb.Usuario.PinHash))
            return Unauthorized("PIN incorrecto");

        // 2.d — jerarquía ROOT > ADMIN > EMPLOYEE
        bool allowed = dto.AgentType switch
        {
            AgentKind.Admin    => usb.Rol is UsbRole.Root or UsbRole.Admin,
            AgentKind.Employee => true,
            _                  => false
        };
        if (!allowed) return Forbid();

        return Ok(new
        {
            cipher = Convert.ToBase64String(usb.RpCipher),
            tag    = Convert.ToBase64String(usb.RpTag),
            rol    = usb.Rol
        });
    }
}

public enum AgentKind { Admin, Employee }
