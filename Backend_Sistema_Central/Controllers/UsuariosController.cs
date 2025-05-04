using Backend_Sistema_Central.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend_Sistema_Central.Dtos;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController(ApplicationDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Crear(Usuario u)
    {
        u.PinHash = BCrypt.Net.BCrypt.HashPassword(u.PinHash);
        db.Usuarios.Add(u);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = u.Id }, u);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Usuario>> Get(int id)
    {
        var usuario = await db.Usuarios.Include(u => u.USBs).FirstOrDefaultAsync(u => u.Id == id);
        return usuario is null ? NotFound() : usuario;
    }
    // ✅ NUEVO: Endpoint que necesita el agente local
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetAll()
    {
        var usuarios = await db.Usuarios
            .Select(u => new UsuarioDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Rut = u.Rut,
                Ip = u.Ip,
                Mac = u.Mac
            })
            .ToListAsync();

        return Ok(usuarios);
    }
}
