using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wallet.Services.Contracts;

namespace Wallet.MVC.Controllers
{
    [Authorize]
 
    public class StatsController : Controller
    {
        private readonly IStatsService _statsService;

        public StatsController(IStatsService statsService)
        {
            _statsService = statsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {



            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var model = await _statsService.GetUserStatsAsync(userId, startDate, endDate);

            return View(model);
        }
    }
}
