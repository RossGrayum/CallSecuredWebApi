using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleWebService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class PingController : Controller
    {
        // GET: api/ping
        [HttpGet]
        [AllowAnonymous]
        public string Ping()
        {
            return "Ping successful.";
        }

        // GET: api/ping/secure
        [HttpGet("secure")]
        public string SecurePing()
        {
            return "Secured ping successful.";
        }
    }
}
