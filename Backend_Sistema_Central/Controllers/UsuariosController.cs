using Backend_Sistema_Central.DTOs;
using Backend_Sistema_Central.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController(ApplicationDbContext db) : ControllerBase
{
    /* ------------------------ GET: lista ------------------------ */
    [HttpGet]
    public async Task<IEnumerable<UsuarioDto>> GetAll()
        => await db.Usuarios
                   .Select(u => new UsuarioDto(
                       u.Id,
                       u.Rut,
                       u.Nombre,
                       u.Ip,
                       u.Mac,
                       u.SerialUsb))
                   .ToListAsync();

    /* -------------------- GET: por ID --------------------------- */
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
                              u.SerialUsb))
                          .SingleOrDefaultAsync();

        return dto is null ? NotFound() : Ok(dto);
    }

    /* ----------------------- POST: crear ------------------------ */
    public record CrearDto(string Rut, string Nombre, string Pin);

    [HttpPost]
    public async Task<IActionResult> Create(CrearDto dto)
    {
        var usuario = new Usuario
        {
            Rut     = dto.Rut,
            Nombre  = dto.Nombre,
            PinHash = BCrypt.Net.BCrypt.HashPassword(dto.Pin)
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return Ok(new { usuario.Id });
    }
}
