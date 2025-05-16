// Controllers/AuthController.cs
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Backend_Sistema_Central.DTOs;
using Backend_Sistema_Central.Models;
using Backend_Sistema_Central.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend_Sistema_Central.Controllers;

/// <summary>Helpers locales del archivo.</summary>
internal static partial class AuthHelpers
{
    /// <summary>Vuelca el PIN en <c>/app/pin_dumps/last_pin.txt</c> dentro del contenedor.</summary>
    internal static void DumpPin(string pin)
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "pin_dumps");
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
    // ───────────────────────────────────────── 1) Verificación inicial del USB
    [HttpPost("verify-usb")]
    public async Task<IActionResult> VerifyUsb([FromBody] UsbVerificationDto dto)
    {
        // 1.a – ¿Certificado firmado por la CA?
        if (!certValidator.IsSignedByRoot(dto.CertPem, out X509Certificate2 cert))
            return Unauthorized("Certificado no firmado por la CA");

        // 1.b – Persistimos / actualizamos el dispositivo
        string thumb = Convert.ToHexString(cert.GetCertHash());
        string serial = dto.Serial.ToUpperInvariant(); // 🔥 Forzar casing
        var usb = await db.DispositivosUSB.FirstOrDefaultAsync(u => u.Serial == serial);



        if (usb is null)
        {
            usb = new DispositivoUSB
            {
                Serial     = dto.Serial,
                Thumbprint = thumb,
                FechaAlta  = DateTime.UtcNow
            };
            db.DispositivosUSB.Add(usb);
        }
        else
        {
            usb.Thumbprint = thumb;
            usb.Revoked    = false;
        }
        await db.SaveChangesAsync();

        // 1.c – Generamos challenge y lo recordamos en memoria (serial + cert)
        byte[] challenge = challenges.Create(dto.Serial, cert);
        return Ok(Convert.ToBase64String(challenge));
    }

    // ───────────────────────────────────────── 2) Login con firma del challenge
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // 2.a – ¿Challenge vivo?
        if (!challenges.TryGet(dto.Serial, out X509Certificate2? cert, out string? challB64))
            return Unauthorized("Challenge vencido");

        // 2.b – Firma válida
        if (!certValidator.VerifySignature(cert!, challB64!, dto.SignatureBase64))
            return Unauthorized("Firma inválida");

        // 2.c – PIN
        var usb = await db.DispositivosUSB
                          .Include(u => u.Usuario)
                          .FirstOrDefaultAsync(u => u.Serial == dto.Serial);

        if (usb?.Usuario == null) return Unauthorized("USB sin usuario asignado");

        logger.LogDebug("DEBUG PIN => {Pin}", dto.Pin);
        AuthHelpers.DumpPin(dto.Pin);

        if (!BCrypt.Net.BCrypt.Verify(dto.Pin.Trim(), usb.Usuario.PinHash))
            return Unauthorized("PIN incorrecto");

        // 2.d – Log OK
/*

        db.Logs.Add(new LogActividad
        {
            UsuarioId  = usb.UsuarioId ?? 0,
            TipoEvento = "LoginOK",
            IP         = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
            MAC        = dto.MacAddress,
            FechaHora  = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
*/
        return Ok("Login exitoso");
    }
}



/*IMPORTANTEEEEEE

Esto es para el bash dentro del usb, para validar que la login funciona

# ==== Paso 1: CHALLENGE ====
SERIAL="040174132B54FB6E1E0E"
CERT=$(sed ':a;N;$!ba;s/\n/\\n/g' device_cert.pem | sed 's/"/\\"/g')
JSON=$(printf '{"serial":"%s","certPem":"%s"}' "$SERIAL" "$CERT")

CHALL=$(curl -sk https://10.145.0.75:8443/api/auth/verify-usb \
             -H "Content-Type: application/json" -d "$JSON")

echo "Challenge = $CHALL"

# ==== Paso 2: FIRMA (¡sin tardar más de 2 min!) ====
echo "$CHALL" | base64 -d > challenge.bin
openssl dgst -sha256 -sign device_key.pem -out sig.bin challenge.bin
SIG_B64=$(base64 -w0 sig.bin)

# ==== Paso 3: LOGIN ====
curl -sk https://10.145.0.75:8443/api/auth/login \
     -H "Content-Type: application/json" \
     -d "{
           \"serial\":\"$SERIAL\",
           \"signatureBase64\":\"$SIG_B64\",
           \"pin\":\"1234\",
           \"macAddress\":\"AABBCCDDEEFF\"
         }"


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