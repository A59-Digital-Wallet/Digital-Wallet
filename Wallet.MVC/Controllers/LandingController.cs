using Microsoft.AspNetCore.Mvc;

namespace Wallet.MVC.Controllers
{
    public class LandingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }

}
