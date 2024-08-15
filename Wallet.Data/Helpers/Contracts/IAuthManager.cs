using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;

namespace Wallet.Data.Helpers.Contracts
{
    public interface IAuthManager
    {
        Task<string> GenerateJwtToken(AppUser user);
    }
}
