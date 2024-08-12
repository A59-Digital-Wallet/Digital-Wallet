using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.DTO.Request
{
    public class ManagePermissionsModel
    {
        public string UserId { get; set; }
        public bool CanSpend { get; set; }
        public bool CanAddFunds { get; set; }
    }

}
