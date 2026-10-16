using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PharmaStore.WebApi.Controllers
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
