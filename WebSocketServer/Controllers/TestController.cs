using Microsoft.AspNetCore.Mvc;

namespace WebSocketServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("get")]
        public ActionResult Get()
        {
            return Ok("hellow,world!");
        }
    }
}
