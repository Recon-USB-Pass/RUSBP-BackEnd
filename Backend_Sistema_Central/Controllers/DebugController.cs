using Microsoft.AspNetCore.Mvc;

namespace Backend_Sistema_Central.Controllers
{
    [ApiController]
    [Route("api/debug")]
    public class DebugController : ControllerBase
    {
        /// <summary>
        /// Valida un PIN contra un hash BCrypt.
        /// GET /api/debug/verify-pin?pin=4321&hash=$2a$11$.....
        /// </summary>
        [HttpGet("verify-pin")]
        public IActionResult VerifyPin(string pin, string hash)
        {
            // Realiza la verificación con BCrypt
            bool ok = BCrypt.Net.BCrypt.Verify(pin, hash);
            return Ok(new { pin, hash, result = ok });
        }
    }
}
