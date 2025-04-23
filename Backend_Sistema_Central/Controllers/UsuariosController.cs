using Backend_Sistema_Central.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
}
