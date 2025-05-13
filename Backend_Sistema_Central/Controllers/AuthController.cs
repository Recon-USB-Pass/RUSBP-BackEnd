using Backend_Sistema_Central.DTOs;
using Backend_Sistema_Central.Models;
using Backend_Sistema_Central.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
        ApplicationDbContext  db,
        ICertificateValidator certValidator,
        IChallengeService     challenges) : ControllerBase
{
    // ──────────────────────────────── 1)  Verificación inicial del USB
    [HttpPost("verify-usb")]
    public async Task<IActionResult> VerifyUsb([FromBody] UsbVerificationDto dto)
    {
        //-- 1.a  ¿Firma de nuestra CA?
        if (!certValidator.IsSignedByRoot(dto.CertPem, out X509Certificate2 cert))
            return Unauthorized("Certificado no firmado por la CA");

        //-- 1.b  Persistimos / actualizamos el dispositivo
        string thumb = Convert.ToHexString(cert.GetCertHash());
        var usb = await db.DispositivosUSB.FirstOrDefaultAsync(u => u.Serial == dto.Serial);

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
            usb.Revoked   = false;
        }
        await db.SaveChangesAsync();

        //-- 1.c  Generamos challenge y recordamos *cert+challenge* en memoria
        byte[] challengeBytes = challenges.Create(dto.Serial, cert);   // Nuevo overload
        string challengeB64   = Convert.ToBase64String(challengeBytes);

        return Ok(challengeB64);
    }

    // ──────────────────────────────── 2)  Login con firma del challenge
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        //-- 2.a  ¿Existe challenge pendiente?
        if (!challenges.TryGet(dto.Serial, out X509Certificate2? cert, out string? challengeB64))
            return Unauthorized("Challenge vencido");

        //-- 2.b  Firma válida   (firma = rsa-pubKey(cert).Verify(challenge))
        if (!certValidator.VerifySignature(cert!, challengeB64!, dto.SignatureBase64))
            return Unauthorized("Firma inválida");

        //-- 2.c  PIN
        var usb = await db.DispositivosUSB.Include(u => u.Usuario)
                                          .FirstOrDefaultAsync(u => u.Serial == dto.Serial);
        if (usb?.Usuario is null)        return Unauthorized("USB sin usuario asignado");
        if (!BCrypt.Net.BCrypt.Verify(dto.Pin, usb.Usuario.PinHash))
            return Unauthorized("PIN incorrecto");

        //-- 2.d  Log OK
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