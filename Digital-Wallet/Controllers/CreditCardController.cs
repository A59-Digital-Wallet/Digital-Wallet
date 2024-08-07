using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Digital_Wallet.Controllers
{
    [Route("api/credit")]
    [ApiController]
    public class CreditCardController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Welcome to Digital Wallet API!");
        }
    }
}
