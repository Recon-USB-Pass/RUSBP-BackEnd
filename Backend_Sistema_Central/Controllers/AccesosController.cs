using Microsoft.AspNetCore.Mvc;
using Backend_Sistema_Central.Models;
using Backend_Sistema_Central.DTOs;

namespace Backend_Sistema_Central.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccesosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public AccesosController(ApplicationDbContext db) { _db = db; }

        // POST: api/accesos
        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] AccesoLogDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Rut) ||
                string.IsNullOrWhiteSpace(dto.SerialUsb) ||
                string.IsNullOrWhiteSpace(dto.Ip) ||
                string.IsNullOrWhiteSpace(dto.Mac))
                return BadRequest("Campos requeridos faltan.");

            var log = new AccesoLog
            {
                Rut = dto.Rut,
                SerialUsb = dto.SerialUsb,
                Ip = dto.Ip,
                Mac = dto.Mac,
                PcName = dto.PcName,
                Fecha = DateTime.UtcNow
            };
            _db.Accesos.Add(log);
            await _db.SaveChangesAsync();
            return Ok();
        }

        // GET: api/accesos/ultimo?serial=XXXXX
        [HttpGet("ultimo")]
        public IActionResult Ultimo([FromQuery] string serial)
        {
            var ultimo = _db.Accesos
                .Where(x => x.SerialUsb == serial)
                .OrderByDescending(x => x.Fecha)
                .FirstOrDefault();

            if (ultimo == null) return NotFound();
            return Ok(ultimo);
        }
    }
}
