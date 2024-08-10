using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Bcpg;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Transactions;
using Wallet.DTO.Request;
using Wallet.Services;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;

namespace Wallet.API.Controllers
{
    [ApiController]
    [Route("api/wallet")]
    [Authorize]
    
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpPost]
        
        public async Task<IActionResult> CreateWallet([FromBody] UserWalletRequest wallet)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            
            await _walletService.CreateWallet(wallet, userId);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserWallet>> GetWallet(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var wallet = await _walletService.GetWalletAsync(id, userId);
            if (wallet == null)
            {
                return NotFound();
            }

            return Ok(wallet);
        }

      
    }
}
