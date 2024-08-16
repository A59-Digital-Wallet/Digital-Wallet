using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Wallet.MVC.Models;
using Wallet.Services.Contracts;

namespace Wallet.MVC.Controllers
{
    public class HomeController : Controller
    {

        private readonly IWalletService _walletService;

        public HomeController(IWalletService walletService)
        {
            _walletService = walletService;
        }



        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var wallets = await _walletService.GetUserWalletsAsync(userId);

            var model = new HomeViewModel
            {
                Wallets = wallets.Select(wallet => new WalletViewModel
                {
                    Id = wallet.Id,
                    Name = wallet.Name,
                    Balance = wallet.Balance,
                    Currency = wallet.Currency.ToString()
                }).ToList()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
