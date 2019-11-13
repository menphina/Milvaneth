using Microsoft.AspNetCore.Mvc;

namespace Milvaneth.Server.Controllers
{
    [Route("api/")]
    [Produces("text/plain")]
    [ApiController]
    public class Controller : ControllerBase
    {
        public Controller()
        {
        }

        // GET api/
        [HttpGet]
        public ActionResult<string> Get()
        {
            return $"Milvaneth Api Service Version 1.0\r\n请勿使用浏览器直接访问本网站\r\n";
        }
    }
}
