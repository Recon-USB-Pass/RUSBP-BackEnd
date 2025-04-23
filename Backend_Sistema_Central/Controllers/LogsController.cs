using Backend_Sistema_Central.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_Sistema_Central.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<LogActividad>> Get(int page = 1, int pageSize = 50) =>
        await db.Logs.OrderByDescending(l => l.FechaHora)
                     .Skip((page - 1) * pageSize)
                     .Take(pageSize)
                     .ToListAsync();
}
