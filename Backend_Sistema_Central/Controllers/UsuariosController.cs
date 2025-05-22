using Backend_Sistema_Central.DTOs;
using Backend_Sistema_Central.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController(ApplicationDbContext db) : ControllerBase
{
    public record CrearDto(
        string Rut,
        string Nombre,
        string? Depto,
        string? Email,
        string? Rol,
        string? PinHash,
        string? Pin
    );

    [HttpGet]
    public async Task<IEnumerable<UsuarioDto>> GetAll()
        => await db.Usuarios
            .Select(u => new UsuarioDto(
                u.Id,
                u.Rut,
                u.Nombre,
                u.Ip,
                u.Mac,
                u.SerialUsb,
                u.Depto,
                u.Email,
                u.Rol))
            .ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> GetById(int id)
    {
        var dto = await db.Usuarios
            .Where(u => u.Id == id)
            .Select(u => new UsuarioDto(
                u.Id,
                u.Rut,
                u.Nombre,
                u.Ip,
                u.Mac,
                u.SerialUsb,
                u.Depto,
                u.Email,
                u.Rol))
            .SingleOrDefaultAsync();

        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearDto dto)
    {
        string pinHash = dto.PinHash ??
            (!string.IsNullOrEmpty(dto.Pin) ? BCrypt.Net.BCrypt.HashPassword(dto.Pin) : throw new Exception("PinHash o Pin requerido"));

        // Opcional: validar Rut único
        if (await db.Usuarios.AnyAsync(u => u.Rut == dto.Rut))
            return Conflict("El Rut ya está registrado.");

        var usuario = new Usuario
        {
            Rut = dto.Rut,
            Nombre = dto.Nombre,
            PinHash = pinHash,
            Depto = dto.Depto,
            Email = dto.Email,
            Rol = dto.Rol,
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return Ok(new { usuario.Id, usuario.Rut });
    }
}
