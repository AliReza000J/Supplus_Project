using Microsoft.AspNetCore.Mvc;

namespace PharmaAdmin.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "Online", timestamp = DateTime.UtcNow });
        }
    }
}
