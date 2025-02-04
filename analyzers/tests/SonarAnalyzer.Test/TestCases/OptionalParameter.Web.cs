using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class MyController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get([FromQuery] string msg = "") // Noncompliant - FP
        {
            return Ok(msg);
        }
    }
}
