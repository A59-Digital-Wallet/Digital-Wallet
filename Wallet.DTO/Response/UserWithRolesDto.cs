using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;

namespace Wallet.DTO.Response
{
    public class UserWithRolesDto
    {
        public AppUser User { get; set; }
        public List<string> Roles { get; set; }
    }
}
