using Microsoft.AspNetCore.Mvc;
using Wallet.DTO.Request;
using Wallet.Services.Contracts;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Wallet.MVC.Controllers
{
    public class WalletController : Controller
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserWalletRequest model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.UserData);
                await _walletService.CreateWallet(model, userId);
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
    }
}
