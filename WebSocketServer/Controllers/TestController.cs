using Microsoft.AspNetCore.Mvc;

namespace WebSocketServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("hello")]
        public ActionResult Get()
        {
            return Ok("Please connect ws://localhost:5000/");
        }
    }
}
