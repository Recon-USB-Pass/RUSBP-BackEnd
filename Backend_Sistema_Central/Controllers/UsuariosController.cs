using Backend_Sistema_Central.DTOs;
using Backend_Sistema_Central.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController(ApplicationDbContext db) : ControllerBase
{
    public record CrearDto(
    [property: JsonPropertyName("rut")] string Rut,
    [property: JsonPropertyName("nombre")] string Nombre,
    [property: JsonPropertyName("depto")] string? Depto,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("rol")] string? Rol,
    [property: JsonPropertyName("pinHash")] string? PinHash,
    [property: JsonPropertyName("pin")] string? Pin
    );

    [HttpGet]
    public async Task<IEnumerable<UsuarioDto>> GetAll()
        => await db.Usuarios
            .Include(u => u.USBs) // <--- Solo si tu modelo Usuario tiene la navegación USBs
            .Select(u => new UsuarioDto(
                u.Id,
                u.Rut,
                u.Nombre,
                u.Ip,
                u.Mac,
                u.Depto,
                u.Email,
                u.Rol,
                u.USBs.OrderBy(x => x.Id).Select(x => x.Serial).FirstOrDefault() // <--- O el campo real
            ))
            .ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> GetById(int id)
    {
        var dto = await db.Usuarios
            .Include(u => u.USBs)
            .Where(u => u.Id == id)
            .Select(u => new UsuarioDto(
                u.Id,
                u.Rut,
                u.Nombre,
                u.Ip,
                u.Mac,
                u.Depto,
                u.Email,
                u.Rol,
                u.USBs.OrderBy(x => x.Id).Select(x => x.Serial).FirstOrDefault()
            ))
            .SingleOrDefaultAsync();

        return dto is null ? NotFound() : Ok(dto);
    }



    //  UsuariosController.cs  → devolver Id SIEMPRE
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearDto dto)
    {
        // Imprime el DTO recibido (debug)
        Console.WriteLine($"DTO recibido ► {System.Text.Json.JsonSerializer.Serialize(dto)}");

        // Determina el hash (acepta pin hash directo o lo hashea si viene en plano)
        string pinHash = !string.IsNullOrEmpty(dto.PinHash)
            ? dto.PinHash
            : (!string.IsNullOrEmpty(dto.Pin)
                ? BCrypt.Net.BCrypt.HashPassword(dto.Pin)
                : throw new Exception("PinHash o Pin requerido"));

        Console.WriteLine($"PinHash calculado: {pinHash}");
        System.Diagnostics.Debug.WriteLine($"PinHash calculado: {pinHash}");

        var usuario = new Usuario
        {
            Rut     = dto.Rut,        // ¡Asegúrate que NO sea null ni string.Empty!
            Nombre  = dto.Nombre,
            PinHash = pinHash,
            Depto   = dto.Depto,
            Email   = dto.Email,
            Rol     = dto.Rol,
            // Otros campos según tu modelo...
        };

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return Ok(new { usuario.Id });
    }


}
